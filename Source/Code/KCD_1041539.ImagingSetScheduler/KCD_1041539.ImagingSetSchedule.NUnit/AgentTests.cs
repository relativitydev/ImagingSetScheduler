using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using KCD_1041539.ImagingSetScheduler.Helper;
using System;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class AgentTests
	{
		#region Vars
		IServicesMgr SvcMgr;
		ExecutionIdentity Identity;
		SqlConnection MasterDbConnection;
		SqlConnection WorkspaceDbConnection;
		Objects.ImagingSetScheduler ImagingSetScheduler;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new ServiceManager(conn.RsapiUri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			MasterDbConnection = conn.GetDbConnection(-1);
			WorkspaceDbConnection = conn.GetDbConnection(Connection.WorkspaceArtifactId);
			var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(SvcMgr, Identity, Connection.WorkspaceArtifactId, Connection.ImagingSetSchedulerArtifactId);
			ImagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
		}

		[TestFixtureTearDown]
		protected void TestFixtureTeardown()
		{
			Query.RemoveKcdQueueRecords(MasterDbConnection);
			try
			{
				MasterDbConnection.Close();
				WorkspaceDbConnection.Close();
			}
			catch (Exception)
			{
				//do nothing
			}
		}
		#endregion

		[Test]
		public void RetrieveApplicationWorkspacesTest()
		{
			//arrange

			//act
			var dt = KCD_1041539.ImagingSetScheduler.Database.SqlQueryHelper.RetrieveApplicationWorkspaces(MasterDbConnection);

			//assert
			Assert.IsTrue(dt.Rows.Count > 0);
		}

		[Test]
		public void InsertJobIntoQueueTest()
		{
			//arrange

			//act
			KCD_1041539.ImagingSetScheduler.Database.SqlQueryHelper.InsertIntoJobQueue(MasterDbConnection, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, ImagingSetScheduler.ArtifactId, Connection.WorkspaceArtifactId);
			var dt = Query.GetTableRecordsFromQueue(MasterDbConnection, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE);

			//assert
			Assert.IsTrue(dt.Rows.Count > 0);
		}

		[Test]
		public void SetErrorTest()
		{
			//arrange
			string errorMessage = "test error message";
			string testStatus = "test status";

			//act
			KCD_1041539.ImagingSetScheduler.Database.SqlQueryHelper.SetErrorMessage(WorkspaceDbConnection, errorMessage, testStatus, ImagingSetScheduler.ArtifactId);
			var dt = Query.GetImagingSetSchedulerRecord(WorkspaceDbConnection, ImagingSetScheduler.ArtifactId);

			//assert
			Assert.AreEqual(errorMessage, (string)dt.Rows[0]["Messages"]);
			Assert.AreEqual(testStatus, (string)dt.Rows[0]["Status"]);
		}
	}
}
