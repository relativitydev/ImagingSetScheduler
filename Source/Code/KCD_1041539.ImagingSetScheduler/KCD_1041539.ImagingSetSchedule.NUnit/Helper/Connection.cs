﻿using System;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using kCura.Relativity.Client;
using Relativity.API;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	class Connection
	{
		// Modify these constants to match your own testing environment
		public const int WORKSPACE_ARTIFACT_ID = 1084798;
		public const int IMAGING_SET_SCHEDULER_ARTIFACT_ID = 1045235;

		public Uri RsapiUri { get; set; }

		private const string _SERVER_NAME = "p-dv-vm-sjar-3.kcura.corp";
		private const string _DB_NAME_PREFIX = "EDDS";
		private const string _DB_SERVER_SUFFIX = "\\EDDSINSTANCE001"; // Probably leave blank if you're not running on a TestVM
		private const string _RSAPI_USER_NAME = "relativity.admin@relativity.com";
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

		public IDBContext GetDbContext(int workspaceArtifactId)
		{
            string dbName = _DB_NAME_PREFIX;

			if (workspaceArtifactId > 0)
			{
				dbName += workspaceArtifactId.ToString();
			}
			
			BaseContext baseContext = new Context(_SERVER_NAME+_DB_SERVER_SUFFIX, dbName, _DB_USER_NAME, _DB_PASSWORD);
			IDBContext context = new DBContext(baseContext);
			return context;
		}

	}
}
