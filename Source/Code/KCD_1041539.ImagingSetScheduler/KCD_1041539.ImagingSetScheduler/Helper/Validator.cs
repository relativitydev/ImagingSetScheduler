using System;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.Interfaces;
using Relativity.API;
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

		public bool VerifyIfImagingSetIsCurrentlyRunning(ImagingSet imagingSet)
		{
			if (string.Equals(imagingSet.Status.Status, Constant.ImagingSetStatus.STAGING
, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(imagingSet.Status.Status, Constant.ImagingSetStatus.COMPLETED_WITH_ERRORS
, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(imagingSet.Status.Status, Constant.ImagingSetStatus.COMPLETED
, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(imagingSet.Status.Status, Constant.ImagingSetStatus.STOPPED_BY_USER
, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(imagingSet.Status.Status, Constant.ImagingSetStatus.ERROR_JOB_FAILED, StringComparison.CurrentCultureIgnoreCase))
			{
				return false;
			}
			return true;
		}
	}
}
