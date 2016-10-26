using System;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	public class ImagingSetTests
	{
		#region Vars
		IServicesMgr SvcMgr;
		ExecutionIdentity Identity;
		SqlConnection MasterDbConnection;
		SqlConnection WorkspaceDbConnection;
		Objects.ImagingSetScheduler ImagingSetScheduler;
		Objects.ImagingSet ImagingSet;
		private const int WORKSPACE_ARTIFACT_ID = Helper.TestConstant.WORKSPACE_ARTIFACT_ID;
		private const int IMAGING_SET_SCHEULE_ARTIFACT_ID = Helper.TestConstant.IMAGING_SET_SCHEDULER_ARTIFACT_ID;

		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new Helper.ServiceManager(conn.Rsapiuri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			MasterDbConnection = conn.GetDbConnection(-1);
			WorkspaceDbConnection = conn.GetDbConnection(WORKSPACE_ARTIFACT_ID);
			WorkspaceDbConnection = conn.GetDbConnection(WORKSPACE_ARTIFACT_ID);
			var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, IMAGING_SET_SCHEULE_ARTIFACT_ID);
			ImagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
			var imagingSetDto = RSAPI.RetrieveSingleImagingSet(SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, ImagingSetScheduler.ImagingSetArtifactId);

			ImagingSet = new Objects.ImagingSet(imagingSetDto, ImagingSetScheduler.CreatedByUserId, null); //todo: fix it with fake EddsDbContext 
		}

		[TestFixtureTearDown]
		protected void TestFixtureTeardown()
		{
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
		public void RetrieveImagingSetTest()
		{
			//arrange

			//act
			var imagingSetDto = RSAPI.RetrieveSingleImagingSet(SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, ImagingSetScheduler.ImagingSetArtifactId);

			//assert
			Assert.IsTrue(imagingSetDto[Constant.Guids.Field.ImagingSet.IMAGING_PROFILE].ValueAsSingleObject.ArtifactID > 0);
			Assert.IsFalse(String.IsNullOrEmpty(imagingSetDto[Constant.Guids.Field.ImagingSet.NAME].ValueAsFixedLengthText));
		}


		[Test]
		public void UpdateImagingSetStatusTest()
		{
			//arrange

			//act
			//ImagingSet.UpdateStatus(SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, Constant.ImagingSetScheduleStatus.STATUS_WAITING); //todo: old test. fix it for newer logic.
			var status = Helper.Query.GetImagingSetStatus(ImagingSet.ArtifactId, WorkspaceDbConnection);

			//assert
			Assert.AreEqual("Waiting", status);
		}

		#region ' Tests for API '

		[Test]
		public void RunImagingSetTest_Invalid_APInotpresent()
		{
			//Arrange

			//act
			//	ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, MasterDbConnection, WorkspaceDbConnection, 1);

			Assert.Throws<Exception>(() => ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, MasterDbConnection, WorkspaceDbConnection, true));

			//assert

		}


		[Test]
		[TestCase(-1)]
		public void RunImagingSetTest_edds(int workspaceID)
		{
			//act
			var response = Assert.Throws<ArgumentException>(() => ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, workspaceID, MasterDbConnection, WorkspaceDbConnection, true));

			//assert
			Assert.AreEqual(response.Message, Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
		}


		[Test]
		[TestCase(938392)]
		public void RunImagingSetTest_Invalid_workspace(int workspaceID)
		{
			//act
			var response = Assert.Throws<ArgumentException>(() => ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, workspaceID, MasterDbConnection, WorkspaceDbConnection, true));

			//assert
			//	Assert.AreEqual(response.Message, Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
		}

		

		#endregion
	}
}
