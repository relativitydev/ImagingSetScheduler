using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetScheduler.Helper;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class ImagingSetSchedulerPreLoadTests
	{
		#region Vars
		IServicesMgr SvcMgr;
		ExecutionIdentity Identity;
		SqlConnection MasterDbConnection;
		SqlConnection WorkspaceDbConnection;
		readonly int WorkspaceArtifactId = TestConstant.WORKSPACE_ARTIFACT_ID;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new ServiceManager(conn.Rsapiuri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			MasterDbConnection = conn.GetDbConnection(-1);
			WorkspaceDbConnection = conn.GetDbConnection(WorkspaceArtifactId);
		}

		[TestFixtureTearDown]
		protected void TestFixtureTeardown()
		{
			try
			{
				MasterDbConnection.Close();
				WorkspaceDbConnection.Close();
			}
			catch (System.Exception)
			{
				//do nothing
			}
		}
		#endregion

		[Test]
		public void GetSingularOrPluralTest()
		{
			//arrange

			//act
			var noResults = ImagingSetHandlerQueries.GetPluralOrSingular(0);
			var oneResult = ImagingSetHandlerQueries.GetPluralOrSingular(1);
			var manyResults = ImagingSetHandlerQueries.GetPluralOrSingular(10);

			//assert
			Assert.AreEqual("documents", noResults);
			Assert.AreEqual("document", noResults);
			Assert.AreEqual("documents", manyResults);
		}
	}
}
