using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using KCD_1041539.ImagingSetScheduler.Helper;
using System.Globalization;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class GetNextOccurrenceTests
	{/*
		#region Vars
		IServicesMgr SvcMgr;
		ExecutionIdentity Identity;
		Objects.ImagingSetScheduler ImagingSetScheduler;
		readonly int WorkspaceArtifactId = Connection.WORKSPACE_ARTIFACT_ID;
		readonly int ImagingSetScheuleArtifactId = Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new ServiceManager(conn.RsapiUri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(SvcMgr, Identity, WorkspaceArtifactId, ImagingSetScheuleArtifactId);
			ImagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
		}
		
		#endregion

		[Test]
		public void GetNextOccurrenceTest_1()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};

			DateTime dateToTest = new DateTime(2014, 2, 24); //Monday
			string expectedResult = "02/24/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_2()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 2, 25); //Tuesday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_3()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 2, 26); //Wednesday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_4()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 2, 27); //Thursday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_5()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 2, 28); //Friday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_6()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 3, 1); //Saturday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}

		[Test]
		public void GetNextOccurrenceTest_7()
		{
			//arrange
			DayOfWeek dayOfWeekToTest = DayOfWeek.Monday;
			List<DayOfWeek> days = new List<DayOfWeek>
			{
				dayOfWeekToTest
			};
			DateTime dateToTest = new DateTime(2014, 3, 2); //Sunday
			string expectedResult = "03/03/2014";

			//act
			var nextOccurrence = ImagingSetScheduler.GetNextOccurrence(days, dateToTest);

			//assert
			Assert.AreEqual(expectedResult, nextOccurrence.ToString("d", DateTimeFormatInfo.InvariantInfo));
		}*/
	}
}
