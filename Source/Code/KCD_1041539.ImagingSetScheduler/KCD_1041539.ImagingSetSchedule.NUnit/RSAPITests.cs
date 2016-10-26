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
	class RSAPITests
	{

		#region Vars
		IServicesMgr _svcMgr;
		ExecutionIdentity _identity;
		SqlConnection _masterDBConnection;
		SqlConnection _workspaceDBConnection;
		int _workspaceArtifactID = Helper.TestConstant.WORKSPACE_ARTIFACT_ID;
		int _imagingSetScheuleArtifactID = Helper.TestConstant.IMAGING_SET_SCHEDULER_ARTIFACT_ID;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			_masterDBConnection = conn.GetDbConnection(-1);
			_workspaceDBConnection = conn.GetDbConnection(_workspaceArtifactID);
			_svcMgr = new Helper.ServiceManager(conn.Rsapiuri, conn.GetRsapi());
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
			catch (System.Exception ex)
			{
				//do nothing
			}
		}
		#endregion

		[Test]
		public void RetrieveImagingSetScheduleTest()
		{
			//arrange
			DTOs.RDO result;

			//act
			result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
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
			result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
			Objects.ImagingSetScheduler imagingSetScheduler = new Objects.ImagingSetScheduler(result);
			imagingSetScheduler.Update(_svcMgr, _identity, _workspaceArtifactID, null, null, "", Constant.ImagingSetSchedulerStatus.COMPLETED_AT);

			//act
			imagingSetSchedulesToCheck = RSAPI.RetrieveAllImagingSetSchedulesNotWaiting(_svcMgr, _identity, _workspaceArtifactID);

			//assert
			Assert.IsTrue(imagingSetSchedulesToCheck.ToList().Count > 0);
		}


	}
}
