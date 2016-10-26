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
			ServerName = "cd-9-3-001"; //"dv-dsk-kyeak3-5.kcura.corp";
			Rsapiuri = new Uri("http://" + ServerName + "/relativity.services/");
			RsapiUserName = "relativity.admin@kcura.com";
			RsapiPassword = "Test1234!";
			DbUserName = "eddsdbo";
			DbPassword = "P@ssw0rd@1";
		}

		public RSAPIClient GetRsapi()
		{
			RSAPIClient client = new RSAPIClient(Rsapiuri, new UsernamePasswordCredentials(RsapiUserName, RsapiPassword));
			client.Login();
			return client;
		}

		public SqlConnection GetDbConnection(int workspaceArtifactId)
		{
			string dbName = "EDDS";

			if (workspaceArtifactId > 0)
			{
				dbName += workspaceArtifactId.ToString();
			}
			var connectionString = String.Format("Server={0};Database={1};User Id={2}; Password={3};", ServerName, dbName, DbUserName, DbPassword);
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

	}
}
