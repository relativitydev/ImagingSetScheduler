using System;
using System.Data.SqlClient;
using kCura.Relativity.Client;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	class Connection
	{
		// Modify these constants to match your own testing environment
		public const int WorkspaceArtifactId = 1332098;
		public const int ImagingSetSchedulerArtifactId = 1040320;

		public Uri RsapiUri { get; set; }

		const string ServerName = "p-dv-vm-changeme-1";
		const string DbNamePrefix = "EDDS";
		const string DbServerSuffix = "\\EDDSINSTANCE001"; // Probably leave blank if you're not running on a TestVM
		const string RsapiUserName = "relativity.admin@kcura.com";
		const string RsapiPassword = "Test1234!";
		const string DbUserName = "eddsdbo";
		const string DbPassword = "MySqlPassword123"; // Possibly P@ssw0rd@1

		public Connection()
		{
			RsapiUri = new Uri("http://" + ServerName + "/relativity.services/");
        }

		public RSAPIClient GetRsapi()
		{
			RSAPIClient client = new RSAPIClient(RsapiUri, new UsernamePasswordCredentials(RsapiUserName, RsapiPassword));
			client.Login();
			return client;
		}

		public SqlConnection GetDbConnection(int workspaceArtifactId)
		{
            string dbName = DbNamePrefix;

			if (workspaceArtifactId > 0)
			{
				dbName += workspaceArtifactId.ToString();
			}
			var connectionString = String.Format("Server={0}{1};Database={2};User Id={3}; Password={4};", ServerName, DbServerSuffix, dbName, DbUserName, DbPassword);
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

	}
}
