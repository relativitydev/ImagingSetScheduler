using System;
using System.Data.SqlClient;
using kCura.Relativity.Client;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	class Connection
	{
		string ServerName { get; set; }
		public Uri Rsapiuri { get; set; }
		string RsapiUserName { get; set; }
		string RsapiPassword { get; set; }
		string DbUserName { get; set; }
		string DbPassword { get; set; }

		public Connection()
		{
			ServerName = "p-dv-vm-jkni-4";
			Rsapiuri = new Uri("http://" + ServerName + "/relativity.services/");
			RsapiUserName = "relativity.admin@kcura.com";
			RsapiPassword = "Test1234!";
			DbUserName = "eddsdbo";
			DbPassword = "MySqlPassword123"; // Possibly P@ssw0rd@1
        }

		public RSAPIClient GetRsapi()
		{
			RSAPIClient client = new RSAPIClient(Rsapiuri, new UsernamePasswordCredentials(RsapiUserName, RsapiPassword));
			client.Login();
			return client;
		}

		public SqlConnection GetDbConnection(int workspaceArtifactId)
		{
		    string dbServerSuffix = "\\EDDSINSTANCE001"; // Probably leave blank if you're not running on a TestVM
            string dbName = "EDDS";

			if (workspaceArtifactId > 0)
			{
				dbName += workspaceArtifactId.ToString();
			}
			var connectionString = String.Format("Server={0}{1};Database={2};User Id={3}; Password={4};", ServerName, dbServerSuffix, dbName, DbUserName, DbPassword);
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

	}
}
