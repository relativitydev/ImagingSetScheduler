using System;
using System.Linq;
using Relativity.API;
using System.Data;
using KCD_1041539.ImagingSetScheduler.Helper;
using DTOs = kCura.Relativity.Client.DTOs;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetScheduler.Database;

namespace KCD_1041539.ImagingSetScheduler.Agents
{
	[kCura.Agent.CustomAttributes.Name("KCD_1041539 - Imaging Set Scheduler Manager")]
	[System.Runtime.InteropServices.Guid("F5E3FD48-38C2-457F-ACBF-5618A30E2957")]
	class Manager : kCura.Agent.AgentBase
	{
		private const String AGENT_TYPE = "Manager Agent";

		public override void Execute()
		{
			RaiseMessage("Agent execution started.", 10);

			IAgentHelper agentHelper = Helper;
			IDBContext eddsDbContext = agentHelper.GetDBContext(-1);
			IServicesMgr svcMgr = ServiceUrlHelper.SetupServiceUrl(eddsDbContext, agentHelper);

			ExecutionIdentity identity = ExecutionIdentity.System;
			var sqlQueryHelper = new SqlQueryHelper();

			try
			{
				RaiseMessage("Retrieving all workspaces where application is installed", 10);
				var workspaceDataTable = RetrieveApplicationWorkspaces(eddsDbContext.GetConnection());

				if (workspaceDataTable.Rows.Count > 0)
				{
					foreach (DataRow workspaceRow in workspaceDataTable.Rows)
					{
						ProcessWorkspace(workspaceRow, svcMgr, identity, eddsDbContext);
					}
				}
				else
				{
					RaiseMessage(String.Format("No workspaces found with {0} application installed", Constant.IMAGING_SET_SCHEDULER_APPLICATION_NAME), 10);
				}
			}
			catch (Exception ex)
			{
				String errorMessages = ex.ToString();
				RaiseMessage(errorMessages, 1);

				var errorcontext = String.Format("{0}. \n\nStack Trace:{1}", AGENT_TYPE, ex);
				sqlQueryHelper.InsertRowIntoErrorLog(eddsDbContext, 0, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, 0, AgentID, errorcontext);
			}

			RaiseMessage("Agent execution finished.", 10);
		}

		private void ProcessWorkspace(DataRow workspaceRow, IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext)
		{
			int workspaceArtifactId = 0;

			try
			{
				workspaceArtifactId = (int)workspaceRow["ArtifactId"];

				RaiseMessage(String.Format("Retrieving all imaging set schedules in workspace which are not in waiting status [WorkspaceArtifactID={0}]", workspaceArtifactId), 10);

				var imagingSetSchedulesToCheck = RSAPI.RetrieveAllImagingSetSchedulesNotWaiting(svcMgr, identity, workspaceArtifactId).ToList();

				if (imagingSetSchedulesToCheck.Count > 0)
				{
					foreach (DTOs.RDO imagingSetSchedulerRdo in imagingSetSchedulesToCheck)
					{
						ProcessImagingSetScheduler(imagingSetSchedulerRdo, svcMgr, identity, eddsDbContext, workspaceArtifactId);
					}
				}
				else
				{
					RaiseMessage(String.Format("No imaging set schedules to check in workspace [WorkspaceArtifactID={0}]", workspaceArtifactId), 10);
				}
			}
			catch (Exception ex)
			{
				var errorContext = String.Format("An error occured when processing Imaging Set Scheduler in a workspace. [WorkspaceArtifactId: {0}]",
					workspaceArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		private void ProcessImagingSetScheduler(DTOs.RDO imagingSetSchedulerRdo, IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext, int workspaceArtifactId)
		{
			int currentImagingSetSchedulerArtifactId = 0;
			DateTime? nextRunDate = null;

			try
			{
				currentImagingSetSchedulerArtifactId = imagingSetSchedulerRdo.ArtifactID;

				if (imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].ValueAsDate.HasValue)
				{
					nextRunDate = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].ValueAsDate.Value;
				}

				if (nextRunDate.HasValue && nextRunDate <= DateTime.Now)
				{
					InsertIntoKcdQueue(imagingSetSchedulerRdo, svcMgr, identity, eddsDbContext, workspaceArtifactId);
				}
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (currentImagingSetSchedulerArtifactId > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId).GetConnection(), errorMessages, currentImagingSetSchedulerArtifactId);
				}

				RaiseMessage(errorMessages, 1);

				var errorContext = String.Format("An error occured when processing Imaging Set Scheduler. [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
					workspaceArtifactId,
					currentImagingSetSchedulerArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		public override string Name
		{
			get
			{
				return "KCD_1041539 - Imaging Set Scheduler Manager";
			}
		}

		public DataTable RetrieveApplicationWorkspaces(SqlConnection dbContext)
		{
			return SqlQueryHelper.RetrieveApplicationWorkspaces(dbContext);
		}

		public void SetError(SqlConnection dbContext, string errorMessage, int imagingSetArtifactId)
		{
			SqlQueryHelper.SetErrorMessage(dbContext, errorMessage, Constant.ImagingSetSchedulerStatus.MANAGER_ERROR, imagingSetArtifactId);
		}

		public void InsertIntoKcdQueue(DTOs.RDO imagingSetSchedulerRdo, IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext, int workspaceArtifactId)
		{
			try
			{
				var imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerRdo);

				RaiseMessage(String.Format("Checking to see if the imaging set schedule is ready to run [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactID={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), 10);

				imagingSetScheduler.InsertRecordIntoQueue(eddsDbContext.GetConnection(), imagingSetScheduler.ArtifactId, workspaceArtifactId);

				imagingSetScheduler.Update(svcMgr, identity, workspaceArtifactId, null, null, "", Constant.ImagingSetSchedulerStatus.WAITING);

				RaiseMessage(String.Format("Imaging set scheduler added to Worker queue [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactID={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), 10);
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (imagingSetSchedulerRdo.ArtifactID > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId).GetConnection(), errorMessages, imagingSetSchedulerRdo.ArtifactID);
				}

				RaiseMessage(errorMessages, 1);
			}
		}
	}
}
