using System;
using System.Collections.Generic;
using System.Data;
using Relativity.API;
using KCD_1041539.ImagingSetScheduler.Helper;
using System.Data.SqlClient;
using Castle.Windsor;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.Interfaces;
using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.IoC;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetScheduler.Agents
{
	[kCura.Agent.CustomAttributes.Name("KCD_1041539 - Imaging Set Scheduler Worker")]
	[System.Runtime.InteropServices.Guid("F596E9E3-5A28-408B-9FAA-334F35DE94A0")]
	public class Worker : kCura.Agent.AgentBase
	{
		private const String AGENT_TYPE = "Worker Agent";
        private object _lock = new object();
        private IContextContainerFactory _contextContainerFactory;
        private IObjectManagerHelper _objectManagerHelper;
        private IAgentHelper _agentHelper;
        private IWindsorContainer _windsorContainer;

        public IAgentHelper AgentHelper => _agentHelper ?? (_agentHelper = Helper);

        public Worker()
        {
            OnAgentDisabled += ReleaseDependencies;
        }

		public override void Execute()
		{
			RaiseMessage("Agent execution started.", 10);

			ResolveDependencies();
            _contextContainerFactory = new ContextContainerFactory(AgentHelper);
            IContextContainer contextContainer = _contextContainerFactory.BuildContextContainer();
            _objectManagerHelper = new ObjectManagerHelper();
			IServicesMgr svcMgr = ServiceUrlHelper.SetupServiceUrl(contextContainer.MasterDbContext, AgentHelper);

			ExecutionIdentity identity = ExecutionIdentity.System;
			var sqlQueryHelper = new SqlQueryHelper();
			
			if (IsCurrentVersionAfterPrairieSmokeRelease(AgentHelper))
			{
				RaiseMessage("Imaging Set Scheduler Worker Agent has been deprecated from Prairie Smoke release.", 10);
			}
			else
			{
				try
				{
					RaiseMessage("Retrieving next imaging set scheduler in waiting status", 10);

					var nextJob = SqlQueryHelper.RetrieveNextJobInQueue(contextContainer.MasterDbContext, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE);

					if (nextJob != null && nextJob.Rows.Count > 0 && nextJob.Rows[0]["ImagingSetSchedulerArtifactId"].ToString() != "")
					{
						ProcessImagingSetSchedulerJob(svcMgr, identity, contextContainer, nextJob);
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
					sqlQueryHelper.InsertRowIntoErrorLog(contextContainer.MasterDbContext, 0, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, 0, AgentID, errorcontext);
				}

				RaiseMessage("Agent execution finished.", 10);
			}
		}

		private void ProcessImagingSetSchedulerJob(IServicesMgr svcMgr, ExecutionIdentity identity, IContextContainer contextContainer, DataTable nextJob)
		{
			int workspaceArtifactId = 0;
			int imagingSetSchedulerArtifactId = 0;
			IValidator validator = new Validator();

			try
			{
				if (nextJob.Rows[0]["WorkspaceArtifactId"] == null)
				{
					throw new CustomExceptions.ImagingSetSchedulerException(String.Format("WorkspaceArtifactId is NULL in {0} table", Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE));
				}

				if (nextJob.Rows[0]["ImagingSetSchedulerArtifactId"] == null)
				{
					throw new CustomExceptions.ImagingSetSchedulerException(String.Format("ImagingSetSchedulerArtifactId is NULL in {0} table", Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE));
				}

				workspaceArtifactId = (int)nextJob.Rows[0]["WorkspaceArtifactId"];

				if (!validator.DoesWorkspaceExists(svcMgr, identity, workspaceArtifactId))
				{
					var errorContext = String.Format("{0}. [WorkspaceArtifactid: {1}]", Constant.ErrorMessages.WORKSPACE_DOES_NOT_EXIST, workspaceArtifactId);
					throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
				}

				imagingSetSchedulerArtifactId = (int)nextJob.Rows[0]["ImagingSetSchedulerArtifactId"];

				RaiseMessage(String.Format("Initializing imaging set scheduler job [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetSchedulerArtifactId, workspaceArtifactId), 10);

				//var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(svcMgr, identity, workspaceArtifactId, imagingSetSchedulerArtifactId);
                var imagingSetSchedulerRelativityObject = _objectManagerHelper.RetrieveSingleImagingSetScheduler(workspaceArtifactId, contextContainer, imagingSetSchedulerArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();


				//validate imaging set scheduler
				//validator.ValidateImagingSetScheduler(imagingSetSchedulerDto);

				var imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerRelativityObject);

				RaiseMessage(String.Format("Submitting imaging set scheduler job [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactId={1}]", imagingSetSchedulerArtifactId, workspaceArtifactId), 10);

				SubmitImagingSetToRunAsync(imagingSetScheduler, workspaceArtifactId, svcMgr, identity, contextContainer.MasterDbContext, validator).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (imagingSetSchedulerArtifactId > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId), errorMessages, imagingSetSchedulerArtifactId);
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

		public void SetError(IDBContext workspaceDbContext, string errorMessage, int imagingSetArtifactId)
		{
			SqlQueryHelper.SetErrorMessage(workspaceDbContext, errorMessage, Constant.ImagingSetSchedulerStatus.COMPLETE_WITH_ERRORS, imagingSetArtifactId);
		}

		public async Task SubmitImagingSetToRunAsync(Objects.ImagingSetScheduler imagingSetScheduler, int workspaceArtifactId, IServicesMgr svcMgr, ExecutionIdentity identity, IDBContext eddsDbContext, IValidator validator)
		{
			try
            {
                var imagingSet = await ImagingApiHelper.RetrieveSingleImagingSetAsync(svcMgr, identity, workspaceArtifactId, imagingSetScheduler.ArtifactId).ConfigureAwait(false);

				//check if the ImagingSet is currently running. If its running, skip current execution.
				bool isImagingSetCurrentlyRunning = validator.VerifyIfImagingSetIsCurrentlyRunning(imagingSet);
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
					await ImagingApiHelper.RunImagingSetAsync(imagingSet, svcMgr, identity, workspaceArtifactId, imagingSetScheduler.LockImagesForQc, imagingSetScheduler.CreatedByUserId).ConfigureAwait(false);

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
				imagingSetScheduler.RemoveRecordFromQueue(imagingSetScheduler.ArtifactId, eddsDbContext, workspaceArtifactId);
			}
		}
		private bool IsCurrentVersionAfterPrairieSmokeRelease(IHelper helper)
		{
			bool isR1Instance = VersionCheckHelper.IsCloudInstanceEnabled(helper);
			bool versionCheckResult = VersionCheckHelper.VersionCheck(helper, Constant.Version.PRAIRIE_SMOKE_VERSION);
			return (isR1Instance && versionCheckResult);
		}
        private void ResolveDependencies()
        {
            if (_windsorContainer == null)
            {
                lock (_lock)
                {
                    if (_windsorContainer == null)
                    {
                        try
                        {
                            var windsorFactory = new WindsorFactory();
                            _windsorContainer = windsorFactory.GetWindsorContainer(AgentHelper);
                            _contextContainerFactory = _windsorContainer.Resolve<IContextContainerFactory>();
                            _objectManagerHelper = _windsorContainer.Resolve<IObjectManagerHelper>();
                        }
                        catch (Exception)
                        {
                            if (_windsorContainer != null)
                            {
                                _windsorContainer.Dispose();
                                _windsorContainer = null;
                            }
                            throw;
                        }
                    }
                }
            }
		}

        private void ReleaseDependencies()
        {
            if (_windsorContainer != null)
            {
                lock (_lock)
                {
                    if (_windsorContainer != null)
                    {
                        _windsorContainer.Release(_contextContainerFactory);
                        _windsorContainer.Release(_objectManagerHelper);
                        _windsorContainer.Dispose();
                        _windsorContainer = null;
                    }
                }
            }
        }
	}
}
