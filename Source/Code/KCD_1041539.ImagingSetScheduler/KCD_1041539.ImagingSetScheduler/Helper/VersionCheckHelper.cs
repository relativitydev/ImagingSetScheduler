
using System;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class VersionCheckHelper
	{
		/// <summary>
		/// Checks if the current Relativity version is greater than target version
		/// </summary>
		/// <param name="helper">A <see cref="IHelper"/>.</param>
		/// <param name="targetVersion">A string of target version.</param>
		/// <returns>A boolean to indicate version check result.</returns>
		public static bool VersionCheck(IHelper helper, string targetVersion)
		{
			const string sql = "SELECT [Value] FROM [Relativity] WHERE [Key] = 'Version'";
			string currentReleaseVersion = helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql);
			Version currentVersion = Version.Parse(currentReleaseVersion);
			Version version = Version.Parse(targetVersion);
			return (currentVersion >= version);
		}

		/// <summary>
		/// Checks if the cloud instance setting is enabled.
		/// </summary>
		/// <param name="helper">A <see cref="IHelper"/></param>
		/// <returns>A boolean to indicate could instance setting.</returns>
		public static bool IsCloudInstanceEnabled(IHelper helper)
		{
			const string sql = "SELECT [Value] FROM [InstanceSetting] WHERE [Name] = 'CloudInstance' AND [Section] = 'Relativity.Core'";
			string instanceSettingValue = helper.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql) ?? "false";
			bool retVal;
			if (!bool.TryParse(instanceSettingValue.ToLower(), out retVal))
			{
				retVal = false;
			}
			return retVal;
		}
	}
}
