using System;
using System.Data;
using Relativity.API;
using KCD_1041539.ImagingSetScheduler.Helper;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.Interfaces;

namespace KCD_1041539.ImagingSetScheduler.Agents
{
	[kCura.Agent.CustomAttributes.Name("KCD_1041539 - Imaging Set Scheduler Worker")]
	[System.Runtime.InteropServices.Guid("F596E9E3-5A28-408B-9FAA-334F35DE94A0")]
	public class Worker : kCura.Agent.AgentBase
	{
		private const String AGENT_TYPE = "Worker Agent";

		public override void Execute()
		{
			RaiseMessage("Agent execution started.", 10);

			IAgentHelper agentHelper = Helper;
			IDBContext eddsDbContext = agentHelper.GetDBContext(-1);
			IServicesMgr svcMgr = ServiceUrlHelper.SetupServiceUrl(eddsDbContext, agentHelper);

			ExecutionIdentity identity = ExecutionIdentity.System;
			var sqlQueryHelper = new SqlQueryHelper();
			SqlConnection eddsSqlConnection = eddsDbContext.GetConnection();

			try
			{
				RaiseMessage("Retrieving next imaging set scheduler in waiting status", 10);

				var nextJob = SqlQueryHelper.RetrieveNextJobInQueue(eddsSqlConnection, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE);

				if (nextJob != null && nextJob.Rows.Count > 0 && nextJob.Rows[0]["ImagingSetSchedulerArtifactId"].ToString() != "")
				{
					ProcesstImagingSetSchedulerJob(svcMgr, identity, eddsDbContext, nextJob);
				}
				else
				{
					RaiseMessage("No imaging set schedules ready to run", 10);
				}
			}
			catch (Exception ex)
			{
				String errorMessages = ex.ToString();
				RaiseMessage(errorMessages, 1);

				var errorcontext = String.Format("{0}. \n\nStack Trace:{1}", AGENT_TYPE, ex);
				sqlQueryHelper.InsertRowIntoErrorLog(eddsDbContext, 0, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE, 0, AgentID, errorcontext);
			}

			RaiseMessage("Agent execution finished.", 10);
		}

		private void ProcesstImagingSetSchedulerJob(IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext, DataTable nextJob)
		{
			int workspaceArtifactId = 0;
			int imagingSetSchedulerArtifactId = 0;
			IValidator validator = new Validator();

			try
			{
				if (nextJob.Rows[0]["WorkspaceArtifactId"] == null)
				{
					throw new CustomExceptions.ImagingSetSchedulerException(String.Format("WorkspaceArtifactId is NULL in {0} table", Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE));
				}

				if (nextJob.Rows[0]["ImagingSetSchedulerArtifactId"] == null)
				{
					throw new CustomExceptions.ImagingSetSchedulerException(String.Format("ImagingSetSchedulerArtifactId is NULL in {0} table", Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE));
				}

				workspaceArtifactId = (int)nextJob.Rows[0]["WorkspaceArtifactId"];

				if (!validator.DoesWorkspaceExists(svcMgr, identity, workspaceArtifactId))
				{
					var errorContext = String.Format("{0}. [WorkspaceArtifactid: {1}]", Constant.ErrorMessages.WORKSPACE_DOES_NOT_EXIST, workspaceArtifactId);
					throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
				}

				imagingSetSchedulerArtifactId = (int)nextJob.Rows[0]["ImagingSetSchedulerArtifactId"];

				RaiseMessage(String.Format("Initializing imaging set scheduler job [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetSchedulerArtifactId, workspaceArtifactId), 10);

				var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(svcMgr, identity, workspaceArtifactId, imagingSetSchedulerArtifactId);

				//validate imaging set scheduler
				validator.ValidateImagingSetScheduler(imagingSetSchedulerDto);

				var imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);

				RaiseMessage(String.Format("Submitting imaging set scheduler job [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetSchedulerArtifactId, workspaceArtifactId), 10);

				SubmitImagingSetToRun(imagingSetScheduler, workspaceArtifactId, svcMgr, identity, eddsDbContext, validator);
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (imagingSetSchedulerArtifactId > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId).GetConnection(), errorMessages, imagingSetSchedulerArtifactId);
				}

				RaiseMessage(errorMessages, 1);

				var errorContext = String.Format("An error occured when processing Imaging Set Scheduler. [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
					workspaceArtifactId,
					imagingSetSchedulerArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		public override string Name
		{
			get
			{
				return "KCD_1041539 - Imaging Set Scheduler Worker";
			}
		}

		public void SetError(SqlConnection dbContext, string errorMessage, int imagingSetArtifactId)
		{
			SqlQueryHelper.SetErrorMessage(dbContext, errorMessage, Constant.ImagingSetSchedulerStatus.COMPLETE_WITH_ERRORS, imagingSetArtifactId);
		}

		public void SubmitImagingSetToRun(Objects.ImagingSetScheduler imagingSetScheduler, int workspaceArtifactId, IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext, IValidator validator)
		{
			try
			{
				var imagingSetRdo = RSAPI.RetrieveSingleImagingSet(svcMgr, identity, workspaceArtifactId, imagingSetScheduler.ImagingSetArtifactId);

				//validate imaging set 
				validator.ValidateImagingSet(imagingSetRdo);

				var imagingSet = new Objects.ImagingSet(imagingSetRdo, imagingSetScheduler.CreatedByUserId, eddsDbContext);

				//check if the ImagingSet is currently running. If its running, skip current execution.
				Boolean isImagingSetCurrentlyRunning = validator.VerifyIfImagingSetIsCurrentlyRunning(imagingSet);
				if (isImagingSetCurrentlyRunning)
				{
					imagingSetScheduler.SetToSkipped(svcMgr, identity, imagingSetScheduler.ArtifactId, workspaceArtifactId);

					RaiseMessage(String.Format("{2} [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]",
						imagingSetScheduler.ArtifactId,
						workspaceArtifactId,
						Constant.ImagingSetSchedulerStatus.SKIPPED), 10);
				}
				else
				{
					imagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, svcMgr, identity, workspaceArtifactId, eddsDbContext.GetConnection(), Helper.GetDBContext(workspaceArtifactId).GetConnection(), imagingSetScheduler.LockImagesForQc);

					imagingSetScheduler.SetToComplete(svcMgr, identity, imagingSetScheduler.ArtifactId, workspaceArtifactId);

					RaiseMessage(String.Format("Imaging set scheduler job is submitted. [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), 10);
				}
			}
			catch (Exception ex)
			{
				var errorMessages = ex.ToString();

				if (imagingSetScheduler.ArtifactId > 0 && workspaceArtifactId > 0)
				{
					imagingSetScheduler.SetToCompleteWithErrors(svcMgr, identity, imagingSetScheduler.ArtifactId, workspaceArtifactId, errorMessages);
				}

				RaiseMessage(errorMessages, 1);

				throw new CustomExceptions.ImagingSetSchedulerException(String.Format("An error occured when submitting Imaging set scheduler job to run. [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), ex);
			}
			finally
			{
				imagingSetScheduler.RemoveRecordFromQueue(imagingSetScheduler.ArtifactId, eddsDbContext.GetConnection(), workspaceArtifactId);
			}
		}
	}
}
