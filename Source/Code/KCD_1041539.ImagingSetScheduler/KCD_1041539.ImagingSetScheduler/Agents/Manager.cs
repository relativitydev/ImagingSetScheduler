using System;
using Relativity.API;
using System.Data;
using Castle.Windsor;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Helper;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.IoC;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetScheduler.Agents
{
	[kCura.Agent.CustomAttributes.Name("KCD_1041539 - Imaging Set Scheduler Manager")]
	[System.Runtime.InteropServices.Guid("F5E3FD48-38C2-457F-ACBF-5618A30E2957")]
	class Manager : kCura.Agent.AgentBase
	{
		private const String AGENT_TYPE = "Manager Agent";
		private object _lock = new object();
		private IContextContainerFactory _contextContainerFactory;
		private IObjectManagerHelper _objectManagerHelper;
		private IAgentHelper _agentHelper;
		private IWindsorContainer _windsorContainer;

		public IAgentHelper AgentHelper => _agentHelper ?? (_agentHelper = Helper);

		public Manager()
		{
			OnAgentDisabled += ReleaseDependencies;
		}

		public override void Execute()
		{
			RaiseMessage("Agent execution started.", 10);

			ResolveDependencies();
			IContextContainer contextContainer = _contextContainerFactory.BuildContextContainer();

			var sqlQueryHelper = new SqlQueryHelper();
			
			if (IsCurrentVersionAfterPrairieSmokeRelease(AgentHelper))
			{
				RaiseMessage("Imaging Set Scheduler Manager Agent has been deprecated from Prairie Smoke release.", 10);
			}
			else
			{
				try
				{
					RaiseMessage("Retrieving all workspaces where application is installed", 10);
					var workspaceDataTable = RetrieveApplicationWorkspaces(contextContainer.MasterDbContext);

					if (workspaceDataTable.Rows.Count > 0)
					{
						foreach (DataRow workspaceRow in workspaceDataTable.Rows)
						{
							ProcessWorkspace(workspaceRow, contextContainer);
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
					sqlQueryHelper.InsertRowIntoErrorLog(contextContainer.MasterDbContext, 0, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, 0, AgentID, errorcontext);
				}

				RaiseMessage("Agent execution finished.", 10);
			}

		}

		private void ProcessWorkspace(DataRow workspaceRow, IContextContainer contextContainer)
		{
			int workspaceArtifactId = 0;

			try
			{
				workspaceArtifactId = (int)workspaceRow["ArtifactId"];

				RaiseMessage(String.Format("Retrieving all imaging set schedules in workspace which are not in waiting status [WorkspaceArtifactID={0}]", workspaceArtifactId), 10);

				var imagingSetSchedulesToCheck = _objectManagerHelper.RetrieveAllImagingSetSchedulesNotWaitingAsync(workspaceArtifactId, contextContainer).ConfigureAwait(false).GetAwaiter().GetResult();

				if (imagingSetSchedulesToCheck.Count > 0)
				{
					foreach (RelativityObject imagingSetSchedulerObject in imagingSetSchedulesToCheck)
					{
						ProcessImagingSetScheduler(imagingSetSchedulerObject, workspaceArtifactId, contextContainer);
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

		private void ProcessImagingSetScheduler(RelativityObject imagingSetSchedulerObject, int workspaceArtifactId, IContextContainer contextContainer)
		{
			int currentImagingSetSchedulerArtifactId = 0;
			DateTime? nextRunDate = null;

			try
			{
				currentImagingSetSchedulerArtifactId = imagingSetSchedulerObject.ArtifactID;

				if (imagingSetSchedulerObject[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].Value != null)
				{
					nextRunDate = (DateTime?)imagingSetSchedulerObject[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].Value;
				}

				if (nextRunDate.HasValue && nextRunDate <= DateTime.Now)
				{
					InsertIntoKcdQueue(imagingSetSchedulerObject, workspaceArtifactId, contextContainer);
				}
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (currentImagingSetSchedulerArtifactId > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId), errorMessages, currentImagingSetSchedulerArtifactId);
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

		public DataTable RetrieveApplicationWorkspaces(IDBContext eddsDbContext)
		{
			return SqlQueryHelper.RetrieveApplicationWorkspaces(eddsDbContext);
		}

		public void SetError(IDBContext workspaceDbContext, string errorMessage, int imagingSetArtifactId)
		{
			SqlQueryHelper.SetErrorMessage(workspaceDbContext, errorMessage, Constant.ImagingSetSchedulerStatus.MANAGER_ERROR, imagingSetArtifactId);
		}

		public void InsertIntoKcdQueue(RelativityObject imagingSetSchedulerObject, int workspaceArtifactId, IContextContainer contextContainer)
		{
			try
			{
				var imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerObject);

				RaiseMessage(String.Format("Checking to see if the imaging set schedule is ready to run [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactID={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), 10);

				imagingSetScheduler.InsertRecordIntoQueue(contextContainer.MasterDbContext, imagingSetScheduler.ArtifactId, workspaceArtifactId);

				imagingSetScheduler.Message = "";
				imagingSetScheduler.Status = Constant.ImagingSetSchedulerStatus.WAITING;

				imagingSetScheduler.Update(workspaceArtifactId, contextContainer, _objectManagerHelper);

				RaiseMessage(String.Format("Imaging set scheduler added to Worker queue [ImagingSetSchedulerArtifactID={0} WorkspaceArtifactID={1}]", imagingSetScheduler.ArtifactId, workspaceArtifactId), 10);
			}
			catch (Exception ex)
			{
				var errorMessages = String.Format("Check Errors tab for any additional error messages. \n\nError Message: {0}", ex);

				if (imagingSetSchedulerObject.ArtifactID > 0 && workspaceArtifactId > 0)
				{
					SetError(Helper.GetDBContext(workspaceArtifactId), errorMessages, imagingSetSchedulerObject.ArtifactID);
				}

				RaiseMessage(errorMessages, 1);
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
