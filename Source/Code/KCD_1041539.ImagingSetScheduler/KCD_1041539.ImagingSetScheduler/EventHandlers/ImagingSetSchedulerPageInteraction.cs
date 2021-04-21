﻿using System;
using kCura.EventHandler;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("10B627AD-43A2-47A2-9413-98A55042A4F3")]
	[kCura.EventHandler.CustomAttributes.Description("Inserts a deprecation warning message on Imaging Set Scheduler RDO.")]
	public class ImagingSetSchedulerPageInteraction : kCura.EventHandler.PageInteractionEventHandler
	{
		private const string _OSIER_RELEASE_MESSAGE =
			"In the Prairie Smoke release ( 7/31/2021 ) Imaging Set Scheduler functionality will be deprecated.<br/>"+
			"Please delete all scheduled imaging jobs in advance and migrate them over to automated workflows to schedule imaging jobs.";

		private const string _PRAIRIE_SMOKE_RELEASE_MESSAGE = "The Imaging Set Scheduler functionality has been deprecated.<br/>" +
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
			const string sql = "SELECT [Value] FROM [Relativity] WHERE [Key] = 'Version'";
			string currentReleaseVersion = Helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql);
			const string prairieSmokeVersion = "12.2";
			const string osierReleaseVersion = "12.1";
			bool isR1Instance = IsCloudInstanceEnabled();

			string deprecationMessage = string.Empty;

			if (isR1Instance)
			{
				if (currentReleaseVersion.StartsWith(prairieSmokeVersion))
				{
					deprecationMessage = _PRAIRIE_SMOKE_RELEASE_MESSAGE;
				}
				else if (currentReleaseVersion.StartsWith(osierReleaseVersion))
				{
					deprecationMessage = _OSIER_RELEASE_MESSAGE;
				}
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
	}
}
