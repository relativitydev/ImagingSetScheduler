using KCD_1041539.ImagingSetScheduler.Helper;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	[TestFixture]
	public class VersionCheckTests
	{
		private Mock<IEHHelper> helper;
		private Mock<IDBContext> dbContext;

		[TestFixtureSetUp]
		protected void TestFixtureSetUp()
		{
			helper = new Mock<IEHHelper>();
			dbContext = new Mock<IDBContext>();
		}

		[TestCase("12.2.16.8", true)]
		[TestCase("12.2", true)]
		[TestCase("11.2.16.8", false)]
		[TestCase("12.1.16.8", false)]
		public void VersionCheckTest(string currentVersion, bool expectedResult)
		{
			//Arrange
			string targetVersion = "12.2";
			string sql = "SELECT [Value] FROM [Relativity] WHERE [Key] = 'Version'";
			helper.Setup(x => x.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql)).Returns(currentVersion);
			
			//Act
			bool result = VersionCheckHelper.VersionCheck(helper.Object, targetVersion);

			//Assert
			Assert.That(result == expectedResult);
		}

		[TestCase("true", true)]
		[TestCase("false", false)]
		public void IsCloudInstanceEnabled(string instanceValue, bool expectedResult)
		{
			//Arrange
			string sql =
				"SELECT [Value] FROM [InstanceSetting] WHERE [Name] = 'CloudInstance' AND [Section] = 'Relativity.Core'";
			helper.Setup(x => x.GetDBContext(-1).ExecuteSqlStatementAsScalar<string>(sql)).Returns(instanceValue);

			//Act
			bool result = VersionCheckHelper.IsCloudInstanceEnabled(helper.Object);

			//Assert
			Assert.That(result == expectedResult);
		}
	}
}
