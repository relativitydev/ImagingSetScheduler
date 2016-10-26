using System;
using kCura.EventHandler;
using KCD_1041539.ImagingSetScheduler.Database;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[System.Runtime.InteropServices.Guid("2D010208-044B-417D-AA94-445336F69F93")]
	[kCura.EventHandler.CustomAttributes.Description("Installs the underlying procedures and tables needed for the imaging set scheduler application.")]
	class WorkspaceSetup : PostInstallEventHandler
	{
		public override Response Execute()
		{
			SqlQueryHelper sqlQueryHelper = new SqlQueryHelper();
			Response retVal = new Response
			{
				Success = true,
				Message = String.Empty
			};

			try
			{
				var eddsDbContext = Helper.GetDBContext(-1);
				sqlQueryHelper.CreateQueueTable(eddsDbContext, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE);
				sqlQueryHelper.CreateErrorLogTable(eddsDbContext);
				sqlQueryHelper.UpdateAgentsToBeOnlyCreatedOnWebServer(eddsDbContext); 
			}
			catch (Exception ex)
			{
				retVal.Success = false;
				retVal.Message = Constant.Messages.MESSAGE_POST_INSTALL_FAILED + ex.Message;
			}
			return retVal;
		}
	}
}
