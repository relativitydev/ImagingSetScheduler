using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using DTOs = kCura.Relativity.Client.DTOs;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;
using KCD_1041539.ImagingSetScheduler.Helper;
namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class RsapiTests
	{

		#region Vars
		IServicesMgr _svcMgr;
		ExecutionIdentity _identity;
		SqlConnection _masterDBConnection;
		SqlConnection _workspaceDBConnection;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			_masterDBConnection = conn.GetDbConnection(-1);
			_workspaceDBConnection = conn.GetDbConnection(Connection.WORKSPACE_ARTIFACT_ID);
			_svcMgr = new Helper.ServiceManager(conn.RsapiUri, conn.GetRsapi());
			_identity = Relativity.API.ExecutionIdentity.System;
		}

		[TestFixtureTearDown]
		protected void TestFixtureTeardown()
		{
			try
			{
				_masterDBConnection.Close();
				_workspaceDBConnection.Close();
			}
			catch (System.Exception)
			{
			}
		}
		#endregion

		[Test]
		public void RetrieveImagingSetScheduleTest()
		{
			//arrange
			DTOs.RDO result;

			//act
			result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			Objects.ImagingSetScheduler imagingSetScheduler = new Objects.ImagingSetScheduler(result);

			//assert
			Assert.IsTrue(result.ArtifactID > 0);
		}

		[Test]
		public void RetrieveAllImagingSetSchedulesTest()
		{
			//arrange
			IEnumerable<DTOs.RDO> imagingSetSchedulesToCheck;
			DTOs.RDO result;
			result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			Objects.ImagingSetScheduler imagingSetScheduler = new Objects.ImagingSetScheduler(result);
			imagingSetScheduler.Update(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, null, null, "", Constant.ImagingSetSchedulerStatus.COMPLETED_AT);

			//act
			imagingSetSchedulesToCheck = RSAPI.RetrieveAllImagingSetSchedulesNotWaiting(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID);

			//assert
			Assert.IsTrue(imagingSetSchedulesToCheck.ToList().Count > 0);
		}


	}
}
