using System;
using KCD_1041539.ImagingSetScheduler.Helper;
using kCura.EventHandler;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("10B627AD-43A2-47A2-9413-98A55042A4F3")]
	[kCura.EventHandler.CustomAttributes.Description("Inserts a deprecation warning message on Imaging Set Scheduler RDO.")]
	public class ImagingSetSchedulerPageInteraction : kCura.EventHandler.PageInteractionEventHandler
	{
		private const string _BEFORE_PRAIRIESMOKE_RELEASE_MESSAGE =
			"Please be advised that starting in the Prairie Smoke release, Imaging Set Scheduler functionality will be deprecated.<br/>"
			+"Please use the Scheduled Run trigger in Automated Workflow to run your scheduled imaging job.";

		private const string _PRAIRIE_SMOKE_RELEASE_MESSAGE = "Please be advised that the Imaging Set Scheduler functionality has been deprecated.<br/>" +
			"Please use Automated Workflows to schedule imaging jobs.";

		public override Response PopulateScriptBlocks()
		{
			Response retVal = new Response {Success = true, Message = string.Empty};

			string warningMessage = BuildDeprecationMessage();

			string htmlToInsert = "<script type=\"text/javascript\"> " +
								  "var messageContainer = window.document.createElement(\"tr\");" +
								  $"messageContainer.innerHTML = '<td colspan=\"3\"><span class=\"dynamicTemplateErrorMessageWidth100\">{warningMessage}</span></td>';" +
								  "var body = document.getElementsByClassName(\"genericArtifactTopActionBar\")[0].tBodies[0];" +
								  "body.insertBefore(messageContainer, body.firstChild);" +
								  "</script>";

			this.RegisterStartupScriptBlock(new ScriptBlock()
			{
				Key = "Warning Message",
				Script = htmlToInsert
			});

			return retVal;
		}

		private string BuildDeprecationMessage()
		{
			bool isR1Instance = VersionCheckHelper.IsCloudInstanceEnabled(Helper);

			string deprecationMessage = string.Empty;

			if (isR1Instance)
			{
				deprecationMessage = VersionCheckHelper.VersionCheck(Helper, ImagingSetScheduler.Helper.Constant.Version.PRAIRIE_SMOKE_VERSION) ? _PRAIRIE_SMOKE_RELEASE_MESSAGE : _BEFORE_PRAIRIESMOKE_RELEASE_MESSAGE;
			}

			return deprecationMessage;
		}
	}
}
