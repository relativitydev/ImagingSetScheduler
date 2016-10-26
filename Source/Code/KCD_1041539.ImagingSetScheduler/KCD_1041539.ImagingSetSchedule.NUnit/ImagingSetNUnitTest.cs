using System;
using kCura.Talos.Utility;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System.Data.SqlClient;
using kCura.Relativity.Client.DTOs;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	class ImagingSetNUnitTest
	{
		#region "Constants"

		private Mock<IHelper> _mockhelper;
		public const string MESSAGE_SAVED_SEARCH_OR_DOC_LIST_POPULATED = "Please select a saved search or enter a value in the Deleted Document List";
		public const String MESSAGE_DELETED_DOCUMENT_LIST_INCORRECTLY_FORMATTED = "Please ensure that the Deleted Document List only includes digits and semi-colons in the following format 1234567;2341412;1234123";
		public Int32 userid = 23455;
		public Int32 workspaceArtifactId = 837627;
		#endregion

		[SetUp]
		public void Setup()
		{
			_mockhelper = new Mock<IHelper>();
		}

		[TearDown]
		public void TearDown()
		{
			_mockhelper = null;
		}

		#region no option present


		[Test]
		[ReportingTest("")]
		[Description("This test throws an exception when there is no saved search or a deleted artifact id list ")]
		public void Presavejob_Savedsearch_DocumentList_null_Fail()
		{
			//Arrange
			var test = new KCD_1041539.ImagingSetScheduler.Objects.ImagingSet(It.IsAny<RDO>(), userid, It.IsAny<IDBContext>());
			//Act

		 test.Run(Constant.SystemArtifactIdentifiers.SYS_ID_IMAGING_SET_JOB_TYPE_FULL, It.IsAny<IServicesMgr>(), ExecutionIdentity.CurrentUser, workspaceArtifactId, It.IsAny<SqlConnection>(), It.IsAny<SqlConnection>(), true);

			//Assert
			

		}


		#endregion no option present

	
	}
}
