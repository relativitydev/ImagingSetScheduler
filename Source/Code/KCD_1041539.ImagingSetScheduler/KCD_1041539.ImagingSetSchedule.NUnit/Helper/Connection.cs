using System;
using System.Data.SqlClient;
using kCura.Relativity.Client;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	class Connection
	{
		// Modify these constants to match your own testing environment
		public const int WORKSPACE_ARTIFACT_ID = 1018362;
		public const int IMAGING_SET_SCHEDULER_ARTIFACT_ID = 1040066;

		public Uri RsapiUri { get; set; }

		private const string _SERVER_NAME = "p-dv-vm-bill-7.kcura.corp";
		private const string _DB_NAME_PREFIX = "EDDS";
		private const string _DB_SERVER_SUFFIX = "\\EDDSINSTANCE001"; // Probably leave blank if you're not running on a TestVM
		private const string _RSAPI_USER_NAME = "relativity.admin@kcura.com";
		private const string _RSAPI_PASSWORD = "Test1234!";
		private const string _DB_USER_NAME = "eddsdbo";
		private const string _DB_PASSWORD = "MySqlPassword123"; // Possibly P@ssw0rd@1

		public Connection()
		{
			RsapiUri = new Uri("https://" + _SERVER_NAME + "/relativity.services/");
        }
		
		public RSAPIClient GetRsapi()
		{
			RSAPIClient client = new RSAPIClient(RsapiUri, new UsernamePasswordCredentials(_RSAPI_USER_NAME, _RSAPI_PASSWORD));
			client.Login();
			return client;
		}

		public SqlConnection GetDbConnection(int workspaceArtifactId)
		{
            string dbName = _DB_NAME_PREFIX;

			if (workspaceArtifactId > 0)
			{
				dbName += workspaceArtifactId.ToString();
			}
			var connectionString = String.Format("Server={0}{1};Database={2};User Id={3}; Password={4};", _SERVER_NAME, _DB_SERVER_SUFFIX, dbName, _DB_USER_NAME, _DB_PASSWORD);
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

	}
}
