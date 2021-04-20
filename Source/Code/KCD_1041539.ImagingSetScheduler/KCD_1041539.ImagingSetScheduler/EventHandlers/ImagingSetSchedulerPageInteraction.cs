using System;
using kCura.EventHandler;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("10B627AD-43A2-47A2-9413-98A55042A4F3")]
	[kCura.EventHandler.CustomAttributes.Description("Inserts a deprecation warning message on Imaging Set Scheduler RDO.")]
	public class ImagingSetSchedulerPageInteraction : kCura.EventHandler.PageInteractionEventHandler
	{
		public override Response PopulateScriptBlocks()
		{
			Response retVal = new Response {Success = true, Message = string.Empty};

			string warningMessage = "WARNING: Test";

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
	}
}
