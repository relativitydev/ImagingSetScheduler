using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using Objects = KCD_1041539.ImagingSetScheduler.Objects;
using KCD_1041539.ImagingSetScheduler.Helper;
using KCD_1041539.ImagingSetSchedule.NUnit.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class GetNextRunDateTests
	{
		#region Vars
		IServicesMgr SvcMgr;
		ExecutionIdentity Identity;
		SqlConnection MasterDbConnection;
		SqlConnection WorkspaceDbConnection;
		Objects.ImagingSetScheduler ImagingSetScheduler;
		private const int WORKSPACE_ARTIFACT_ID = Connection.WORKSPACE_ARTIFACT_ID;
		private readonly int ImagingSetScheuleArtifactId = Connection.IMAGING_SET_SCHEDULER_ARTIFACT_ID;
		#endregion

		#region SetUp and Teardown
		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			Connection conn = new Connection();

			SvcMgr = new ServiceManager(conn.RsapiUri, conn.GetRsapi());
			Identity = ExecutionIdentity.System;
			MasterDbConnection = conn.GetDbConnection(-1);
			WorkspaceDbConnection = conn.GetDbConnection(WORKSPACE_ARTIFACT_ID);
			var imagingSetSchedulerDto = RSAPI.RetrieveSingleImagingSetScheduler(SvcMgr, Identity, WORKSPACE_ARTIFACT_ID, ImagingSetScheuleArtifactId);
			ImagingSetScheduler = new Objects.ImagingSetScheduler(imagingSetSchedulerDto);
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
		public void GetNextRunDateTest_SameDay_BeforeNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday
			};

			var todaysDate = new DateTime(2014, 2, 24, 14, 30, 0);
			var scheduledTime = "13:30";
			var expectedResult = new DateTime(2014, 03, 03, 13, 30, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			if (ImagingSetScheduler.NextRunDate != null) //todo: write is not null assert
			{
				Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
			}
		}

		[Test]
		public void GetNextRunDateTest_SameDay_AfterNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday
			};

			var todaysDate = new DateTime(2014, 2, 24, 14, 30, 0);
			var scheduledTime = "15:30";
			var expectedResult = new DateTime(2014, 2, 24, 15, 30, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_SameDay_ExactlyNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday
			};

			var todaysDate = new DateTime(2014, 2, 24, 14, 30, 0);
			var scheduledTime = "14:30";
			var expectedResult = new DateTime(2014, 2, 24, 14, 30, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_2Days_AfterNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 25, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_2Days_BeforeNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 24, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_2Days_ExactlyNow()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "10:00";
			var expectedResult = new DateTime(2014, 2, 24, 10, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_1()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Tuesday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 25, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_2()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Wednesday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 26, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_3()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Thursday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 27, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_4()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Friday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 28, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_5()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Saturday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 3, 1, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_CurrentDayNotInScheduled_6()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 3, 2, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_1()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 25, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_2()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 25, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 26, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_3()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 26, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 27, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_4()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 27, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 2, 28, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_5()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 28, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 3, 1, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_6()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 3, 1, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 3, 2, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_BeforeNow_7()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 3, 2, 10, 00, 0);
			var scheduledTime = "09:00";
			var expectedResult = new DateTime(2014, 3, 3, 09, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_1()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 24, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 24, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_2()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 25, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 25, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_3()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 26, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 26, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}


		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_4()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 27, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 27, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_5()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 2, 28, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 2, 28, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_6()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 3, 1, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 3, 1, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_7()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Wednesday, 
				DayOfWeek.Thursday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Sunday
			};

			var todaysDate = new DateTime(2014, 3, 2, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 3, 2, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_DifferentOrder()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Thursday, 
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Sunday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Wednesday
			};


			var todaysDate = new DateTime(2014, 3, 2, 10, 00, 0);
			var scheduledTime = "11:00";
			var expectedResult = new DateTime(2014, 3, 2, 11, 00, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}

		[Test]
		public void GetNextRunDateTest_AllDays_AfterNow_DifferentTime()
		{
			//arrange
			List<DayOfWeek> scheduledFrequency = new List<DayOfWeek>
			{
				DayOfWeek.Thursday, 
				DayOfWeek.Monday, 
				DayOfWeek.Tuesday, 
				DayOfWeek.Sunday, 
				DayOfWeek.Friday, 
				DayOfWeek.Saturday, 
				DayOfWeek.Wednesday
			};

			var todaysDate = new DateTime(2014, 2, 27, 11, 29, 0);
			var scheduledTime = "11:30";
			var expectedResult = new DateTime(2014, 2, 27, 11, 30, 0);

			//act
			ImagingSetScheduler.GetNextRunDate(scheduledFrequency, todaysDate, scheduledTime);

			//assert
			Assert.IsNotNull(ImagingSetScheduler.NextRunDate);
			Assert.AreEqual(expectedResult, ImagingSetScheduler.NextRunDate.Value);
		}
	}
}
