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
			const string sql = "SELECT [Value] FROM [Relativity] WHERE [Key] = 'Version'";
			string currentReleaseVersion = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql);
			const string prairieSmokeVersion = "12.2";
			bool isR1Instance = IsCloudInstanceEnabled();

			string deprecationMessage;

			if (isR1Instance && IsIncomingVersionHigherOrEqual(prairieSmokeVersion, currentReleaseVersion))
			{
				//test
				deprecationMessage = "Imaging Set Scheduler is now obsolete and disabled. Check out Imaging Automated Workflows!";
			}
			else
			{
				deprecationMessage = string.Empty;
			}

			return deprecationMessage;
		}

		private bool IsCloudInstanceEnabled()
		{
			const string sql = "SELECT [Value] FROM [InstanceSetting] WHERE [Name] = 'CloudInstance' AND [Section] = 'Relativity.Core'";
			string currentReleaseVersion = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql) ?? "false";
			bool retVal;
			if (!bool.TryParse(currentReleaseVersion.ToLower(), out retVal))
			{
				retVal = false;
			}
			return retVal;
		}

		private bool IsIncomingVersionHigherOrEqual(string targetVersion, string incomingVersion)
		{
			string[] targetVersionParts = targetVersion.Split('.');
			string[] incomingVersionParts = incomingVersion.Split('.');

			int maxIndex;

			if (targetVersionParts.Length != incomingVersionParts.Length)
			{
				// stop at whatever the shortest is to prevent a out of bounds error
				maxIndex = Math.Min(targetVersionParts.Length, incomingVersionParts.Length);
			}
			else
			{
				maxIndex = targetVersion.Length;
			}

			bool isVersionHigher = true;

			// set flag to false when an incoming number is less than our target
			for (int i = 0; i < maxIndex; i++)
			{
				int incomingVersionPart = Convert.ToInt32(incomingVersionParts[i]);
				int targetVersionPart = Convert.ToInt32(targetVersionParts[i]);
				isVersionHigher &= incomingVersionPart >= targetVersionPart;
			}
			return isVersionHigher;
		}
	}
}
