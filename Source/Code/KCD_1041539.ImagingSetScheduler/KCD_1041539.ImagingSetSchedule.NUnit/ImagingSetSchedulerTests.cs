using System;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using DTOs = kCura.Relativity.Client.DTOs;
using System.Data;
using KCD_1041539.ImagingSetScheduler.Helper;
using System.Globalization;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	//[TestFixture]
	class ImagingSetSchedulerTests
	{
		//#region Vars
		/*IServicesMgr _svcMgr;
		ExecutionIdentity _identity;
		IDBContext _masterDBConnection;
		IDBContext _workspaceDBConnection;
		Objects.ImagingSetScheduler _imagingSetScheduler;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			_svcMgr = new Helper.ServiceManager(conn.RsapiUri, conn.GetRsapi());
			_identity = Relativity.API.ExecutionIdentity.System;
			_masterDBConnection = conn.GetDbContext(-1);
			_workspaceDBConnection = conn.GetDbContext(Connection.WORKSPACE_ARTIFACT_ID);
		}

		#endregion

		[Test]
		public void NewImagingSetSchedulerTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);

			//act
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);

			//assert
			Assert.IsNotNullOrEmpty(_imagingSetScheduler.Name);
			Assert.IsTrue(_imagingSetScheduler.ImagingSetArtifactId > 0);
			Assert.IsNotNull(_imagingSetScheduler.LockImagesForQc);
			Assert.IsNotNull(_imagingSetScheduler.LockImagesForQc);
			Assert.IsTrue(_imagingSetScheduler.CreatedByUserId > 0);
			Assert.IsTrue(_imagingSetScheduler.FrequencyList.Count > 0);
			Assert.IsNotNullOrEmpty(_imagingSetScheduler.Time);
		}

		[Test]
		public void UpdateStatusTest()
		{
			//arrange
			DataTable dt;
			DTOs.RDO imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
			var expectedLastRun = DateTime.Now.ToUniversalTime();
			_imagingSetScheduler.LastRunDate = expectedLastRun;
			var expectedNextRun = expectedLastRun.AddDays(1);
			_imagingSetScheduler.NextRunDate = expectedNextRun;

			//act
			_imagingSetScheduler.Update(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, _imagingSetScheduler.LastRunDate, _imagingSetScheduler.NextRunDate, "test message", "test status");
			dt = Helper.Query.GetImagingSetSchedulerRecord(_workspaceDBConnection, _imagingSetScheduler.ArtifactId);
			DateTime actualLastRunDate = (DateTime)dt.Rows[0]["LastRun"];
			DateTime actualNextRunDate = (DateTime)dt.Rows[0]["NextRun"];

			//assert
			DateTimeFormatInfo fmt = (new CultureInfo("hr-HR")).DateTimeFormat;
			Assert.AreEqual(expectedLastRun.ToString("g", fmt), actualLastRunDate.ToString("g", fmt));
			Assert.AreEqual(expectedNextRun.ToString("g", fmt), actualNextRunDate.ToString("g", fmt));
			Assert.AreEqual("test message", (string)dt.Rows[0]["Messages"]);
			Assert.AreEqual("test status", (string)dt.Rows[0]["Status"]);
		}

		[Test]
		public void UpdateStatusAndMessageTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
			DateTimeFormatInfo fmt = (new CultureInfo("hr-HR")).DateTimeFormat;

			//act
			_imagingSetScheduler.Update(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, null, null, "", Constant.ImagingSetSchedulerStatus.WAITING);
			DataTable dt = Helper.Query.GetImagingSetSchedulerRecord(_workspaceDBConnection, _imagingSetScheduler.ArtifactId);

			//assert
			Assert.AreEqual("", (string)dt.Rows[0]["Messages"]);
			Assert.AreEqual(Constant.ImagingSetSchedulerStatus.WAITING, (string)dt.Rows[0]["Status"]);
		}

		[Test]
		public void ConvertStringToDayOfWeekTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);

			//act
			DayOfWeek sundayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Sunday.ToString());
			DayOfWeek mondayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Monday.ToString());
			DayOfWeek tuesdayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Tuesday.ToString());
			DayOfWeek wednesdayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Wednesday.ToString());
			DayOfWeek thursdayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Thursday.ToString());
			DayOfWeek fridayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Friday.ToString());
			DayOfWeek saturdayDayOfWeek = _imagingSetScheduler.ConvertStringToDayOfWeek(DayOfWeek.Saturday.ToString());

			//assert 
			Assert.AreEqual(DayOfWeek.Sunday, sundayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Monday, mondayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Tuesday, tuesdayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Wednesday, wednesdayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Thursday, thursdayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Friday, fridayDayOfWeek);
			Assert.AreEqual(DayOfWeek.Saturday, saturdayDayOfWeek);
		}

		[Test]
		public void RemoveRecordFromQueueTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, Connection.WORKSPACE_ARTIFACT_ID, Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
			KCD_1041539.ImagingSetScheduler.Database.SqlQueryHelper.InsertIntoJobQueue(_masterDBConnection, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, _imagingSetScheduler.ArtifactId, Connection.WORKSPACE_ARTIFACT_ID);

			//act
			_imagingSetScheduler.RemoveRecordFromQueue(_imagingSetScheduler.ArtifactId, _masterDBConnection, Connection.WORKSPACE_ARTIFACT_ID);
			DataTable dt = Helper.Query.GetTableRecordsFromQueue(_masterDBConnection, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE);

			//assert
			Assert.AreEqual(0, dt.Rows.Count);
		}
		*/
	}
}
