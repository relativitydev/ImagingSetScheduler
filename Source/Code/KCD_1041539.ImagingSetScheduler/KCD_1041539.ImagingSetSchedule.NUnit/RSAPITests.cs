using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Relativity.API;
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
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();
			_svcMgr = new Helper.ServiceManager(conn.RsapiUri, conn.GetRsapi());
			_identity = Relativity.API.ExecutionIdentity.System;
		}

		#endregion

		[Test]
		public void RetrieveImagingSetScheduleTest()
		{
			//arrange
			
			//act
			DTOs.RDO result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			Objects.ImagingSetScheduler imagingSetScheduler = new Objects.ImagingSetScheduler(result);

			//assert
			Assert.IsTrue(result.ArtifactID > 0);
		}

		[Test]
		public void RetrieveAllImagingSetSchedulesTest()
		{
			//arrange
			DTOs.RDO result = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			Objects.ImagingSetScheduler imagingSetScheduler = new Objects.ImagingSetScheduler(result);
			imagingSetScheduler.Update(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, null, null, "", Constant.ImagingSetSchedulerStatus.COMPLETED_AT);

			//act
			IEnumerable<DTOs.RDO> imagingSetSchedulesToCheck = RSAPI.RetrieveAllImagingSetSchedulesNotWaiting(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID);

			//assert
			Assert.IsTrue(imagingSetSchedulesToCheck.ToList().Count > 0);
		}


	}
}
