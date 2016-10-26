using System;
using kCura.EventHandler;
using KCD_1041539.ImagingSetScheduler.Helper;
using KCD_1041539.ImagingSetScheduler.Interfaces;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[System.Runtime.InteropServices.Guid("56BEF27D-A34E-4ACC-9D5B-A9FE4DC79F03")]
	[kCura.EventHandler.CustomAttributes.Description("Verifies for supported Relativity version and supported Imaging application version")]
	public class PreInstallValidation : PreInstallEventHandler
	{
		public override Response Execute()
		{
			var retVal = new Response
			{
				Success = true,
				Message = String.Empty
			};

			IValidator validator = new Validator();
			IDBContext eddsDbContext = Helper.GetDBContext(-1);

			if (!validator.VerifyRelativityVersionForImagingSetExternalApi(eddsDbContext))
			{
				retVal.Success = false;
				retVal.Message = String.Format("{0} application is only supported in Relativity version equal to or higher than {1}.",
					Constant.IMAGING_SET_SCHEDULER_APPLICATION_NAME,
					Constant.IMAGING_SET_SCHEDULER_SUPPORTED_RELATIVITY_VERSION);
			}

			return retVal;
		}
	}
}
