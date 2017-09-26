using System;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using KCD_1041539.ImagingSetScheduler.Helper;
using Moq;
using System.Collections.Generic;

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

		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new Helper.ServiceManager(conn.RsapiUri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			MasterDbConnection = conn.GetDbConnection(-1);
			WorkspaceDbConnection = conn.GetDbConnection(Connection.WORKSPACE_ARTIFACT_ID);
			WorkspaceDbConnection = conn.GetDbConnection(Connection.WORKSPACE_ARTIFACT_ID);
			var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(SvcMgr, Identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			ImagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
			var imagingSetDto = RSAPI.RetrieveSingleImagingSet(SvcMgr, Identity, Connection.WORKSPACE_ARTIFACT_ID, ImagingSetScheduler.ImagingSetArtifactId);

			var eddsDbContext = new Mock<IDBContext>();
			eddsDbContext.Setup(x => x.ExecuteSqlStatementAsScalar<Int32>(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>())).Returns(9);

			ImagingSet = new Objects.ImagingSet(imagingSetDto, ImagingSetScheduler.CreatedByUserId, eddsDbContext.Object);
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
			var imagingSetDto = RSAPI.RetrieveSingleImagingSet(SvcMgr, Identity, Connection.WORKSPACE_ARTIFACT_ID, ImagingSetScheduler.ImagingSetArtifactId);

			//assert
			Assert.IsTrue(imagingSetDto[Constant.Guids.Field.ImagingSet.IMAGING_PROFILE].ValueAsSingleObject.ArtifactID > 0);
			Assert.IsFalse(String.IsNullOrEmpty(imagingSetDto[Constant.Guids.Field.ImagingSet.NAME].ValueAsFixedLengthText));
		}

		#region ' Tests for API '

		[Test]
		[TestCase(-1)]
		public void RunImagingSetTest_edds(int workspaceId)
		{
			//act
			var response = Assert.Throws<ArgumentException>(() => ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, workspaceId, MasterDbConnection, WorkspaceDbConnection, true));

			//assert
			Assert.AreEqual(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE, response.Message);
		}


		[Test]
		[TestCase(1)]
		public void RunImagingSetTest_Invalid_workspace(int workspaceId)
		{
			//act
			var response = Assert.Throws<CustomExceptions.ImagingSetSchedulerException>(() => ImagingSet.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, SvcMgr, Identity, workspaceId, MasterDbConnection, WorkspaceDbConnection, true));
		}

		#endregion
	}
}
