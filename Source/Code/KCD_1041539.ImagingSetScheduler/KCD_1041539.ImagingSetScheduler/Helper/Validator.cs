using System;
using kCura.Relativity.Client;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.Interfaces;
using Relativity.API;
using DTOs = kCura.Relativity.Client.DTOs;
using Relativity.Imaging.Services.Interfaces;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class Validator : IValidator
	{
		public SqlQueryHelper SqlQueryHelper { get; set; }

		public Validator()
		{
			SqlQueryHelper = new SqlQueryHelper();
		}


		public void ValidateImagingSetScheduler(DTOs.RDO imagingSetSchedulerRdo)
		{
			String nameField = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.NAME].ValueAsFixedLengthText;
			if (nameField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Name field is NULL.");
			}

			DTOs.Artifact imagingSetField = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET].ValueAsSingleObject;
			if (imagingSetField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Imaging Set field is NULL.");
			}

			DTOs.MultiChoiceFieldValueList frequencyField = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.FREQUENCY].ValueAsMultipleChoice;
			if (frequencyField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Frequency field is NULL.");
			}
			if (frequencyField.Count == 0)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Frequency field is empty.");
			}

			String timeField = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.TIME].ValueAsFixedLengthText;
			if (timeField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Time field is NULL.");
			}

			bool? lockImagesForQcField = imagingSetSchedulerRdo[Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC].ValueAsYesNo;
			if (lockImagesForQcField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Lock Images for QC field is NULL.");
			}
		}

		public bool VerifyRelativityVersionForImagingSetExternalApi(IDBContext eddsDbContext)
		{
			var errorContext = "An error occured when verifying supported Relativity version";

			String relativityVersionString = SqlQueryHelper.GetRelativityVersion(eddsDbContext);
			if (relativityVersionString == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(string.Format("{0}. Current Relativity version is NULL.",
					errorContext));
			}

			var relativityVersion = new Version(relativityVersionString);
			var supportedRelativityVersion = new Version(Constant.IMAGING_SET_SCHEDULER_SUPPORTED_RELATIVITY_VERSION);

			if (relativityVersion == null || supportedRelativityVersion == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(string.Format("{0}. Current Relativity version or supported Relativity version is NULL.",
					errorContext));
			}

			var result = relativityVersion.CompareTo(supportedRelativityVersion);
			if (result < 0)
			{
				return false;
			}
			return true;
		}

		public bool VerifyImagingApplicationVersionForImagingSetExternalApi(IDBContext workspaceDbContext)
		{
			var errorContext = String.Format("An error occured when verifying supported {0} application version",
				Constant.IMAGING_APPLICATION_NAME);

			String applicationVersionString = SqlQueryHelper.GetImagingApplicationVersion(workspaceDbContext);
			if (applicationVersionString == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(string.Format("{0}. Current {1} application version is NULL.",
					errorContext,
					Constant.IMAGING_APPLICATION_NAME));
			}


			var applicationVersion = new Version(applicationVersionString);
			var supportedApplicationVersion = new Version(Constant.IMAGING_SET_SCHEDULER_SUPPORTED_IMAGING_APPLICATION_VERSION);

			if (applicationVersion == null || supportedApplicationVersion == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(string.Format("{0}. Current {1} application version or supported {1} application version is NULL.",
					errorContext,
					Constant.IMAGING_APPLICATION_NAME));
			}

			var result = applicationVersion.CompareTo(supportedApplicationVersion);
			if (result < 0)
			{
				return false;
			}
			return true;
		}

		//TODO#Bill: test this
		public bool VerifyIfImagingSetIsCurrentlyRunning(ImagingSet imagingSet)
		{
			if (imagingSet.Status.Status == Constant.ImagingSetStatus.STAGING
					|| imagingSet.Status.Status == Constant.ImagingSetStatus.COMPLETED_WITH_ERRORS
					|| imagingSet.Status.Status == Constant.ImagingSetStatus.COMPLETED
					|| imagingSet.Status.Status == Constant.ImagingSetStatus.STOPPED_BY_USER
					|| imagingSet.Status.Status == Constant.ImagingSetStatus.ERROR_JOB_FAILED)
			{
				return false;
			}
			return true;
		}

		public bool DoesWorkspaceExists(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId)
		{
			Boolean retVal = RSAPI.DoesWorkspaceExists(svcMgr, identity, workspaceArtifactId);
			return retVal;
		}
	}
}
