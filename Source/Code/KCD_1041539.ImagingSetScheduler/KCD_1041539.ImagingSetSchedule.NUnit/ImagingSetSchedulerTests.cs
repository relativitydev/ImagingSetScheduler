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
	[TestFixture]
	class ImagingSetSchedulerTests
	{
		#region Vars
		IServicesMgr _svcMgr;
		ExecutionIdentity _identity;
		SqlConnection _masterDBConnection;
		SqlConnection _workspaceDBConnection;
		Objects.ImagingSetScheduler _imagingSetScheduler;
		int _workspaceArtifactID = Helper.TestConstant.WORKSPACE_ARTIFACT_ID;
		int _imagingSetScheuleArtifactID = Helper.TestConstant.IMAGING_SET_SCHEDULER_ARTIFACT_ID;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			_svcMgr = new Helper.ServiceManager(conn.Rsapiuri, conn.GetRsapi());
			_identity = Relativity.API.ExecutionIdentity.System;
			_masterDBConnection = conn.GetDbConnection(-1);
			_workspaceDBConnection = conn.GetDbConnection(_workspaceArtifactID);
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
		public void NewImagingSetSchedulerTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDTO;
			imagingSetSchedulerDTO = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);

			//act
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDTO);

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
			DTOs.RDO imagingSetSchedulerDTO;
			imagingSetSchedulerDTO = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDTO);
			_imagingSetScheduler.LastRunDate = DateTime.Now;
			_imagingSetScheduler.NextRunDate = DateTime.Now.AddDays(1);
			DateTimeFormatInfo fmt = (new CultureInfo("hr-HR")).DateTimeFormat;

			//act
			_imagingSetScheduler.Update(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheduler.LastRunDate, _imagingSetScheduler.NextRunDate, "test message", "test status");
			dt = Helper.Query.GetImagingSetSchedulerRecord(_workspaceDBConnection, _imagingSetScheduler.ArtifactId);
			DateTime actualLastRunDate = (DateTime)dt.Rows[0]["LastRun"];
			DateTime actualNextRunDate = (DateTime)dt.Rows[0]["NextRun"];

			//assert
			Assert.AreEqual(_imagingSetScheduler.LastRunDate.Value.ToString("g", fmt), actualLastRunDate.ToString("g", fmt));
			Assert.AreEqual(_imagingSetScheduler.NextRunDate.Value.ToString("g", fmt), actualNextRunDate.ToString("g", fmt));
			Assert.AreEqual("test message", (string)dt.Rows[0]["Messages"]);
			Assert.AreEqual("test status", (string)dt.Rows[0]["Status"]);
		}

		[Test]
		public void UpdateStatusAndMessageTest()
		{
			//arrange
			DataTable dt;
			DTOs.RDO imagingSetSchedulerDTO;
			imagingSetSchedulerDTO = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDTO);
			DateTimeFormatInfo fmt = (new CultureInfo("hr-HR")).DateTimeFormat;

			//act
			_imagingSetScheduler.Update(_svcMgr, _identity, _workspaceArtifactID, null, null, "", Constant.ImagingSetSchedulerStatus.WAITING);
			dt = Helper.Query.GetImagingSetSchedulerRecord(_workspaceDBConnection, _imagingSetScheduler.ArtifactId);

			//assert
			Assert.AreEqual("", (string)dt.Rows[0]["Messages"]);
			Assert.AreEqual(Constant.ImagingSetSchedulerStatus.WAITING, (string)dt.Rows[0]["Status"]);
		}

		[Test]
		public void ConvertStringToDayOfWeekTest()
		{
			//arrange
			DTOs.RDO imagingSetSchedulerDTO;
			imagingSetSchedulerDTO = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDTO);

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
			DTOs.RDO imagingSetSchedulerDTO;
			DataTable dt;
			imagingSetSchedulerDTO = RSAPI.RetrieveSingleImagingSetScheduler(_svcMgr, _identity, _workspaceArtifactID, _imagingSetScheuleArtifactID);
			_imagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDTO);
			KCD_1041539.ImagingSetScheduler.Database.SqlQueryHelper.InsertIntoJobQueue(_masterDBConnection, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE, _imagingSetScheduler.ArtifactId, _workspaceArtifactID);

			//act
			_imagingSetScheduler.RemoveRecordFromQueue(_imagingSetScheduler.ArtifactId, _masterDBConnection, _workspaceArtifactID);
			dt = Helper.Query.GetTableRecordsFromQueue(_masterDBConnection, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE);

			//assert
			Assert.AreEqual(0, dt.Rows.Count);
		}

	}
}
