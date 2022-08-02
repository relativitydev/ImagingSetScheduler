using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Helper;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
    [TestFixture]
    public class ObjectManagerHelperTests
    {
		private Fixture _testFixture;
		private const int DEFAULT_BATCH_SIZE = 1000;
		private const string SECTION = "Relativity.Imaging";
		private const string INSTANCE_SETTING_NAME = "ImagingSetSchedulerBatchSize";
		private ObjectManagerHelper _instance;
		private Mock<IContextContainer> _contextContainer;
		private Mock<IServicesProxyFactory> _serviceProxyFactory;
		private Mock<IObjectManager> _objectManager;
		private Mock<IInstanceSettingManager> _instanceSettingManager;
		private Mock<IWorkspaceManager> _workspaceManager;
		private int _workspaceId;
        private int _imagingSetSchedulerId;
        private DateTime _lastRun;
        private DateTime _nextRun;
        private const string MESSAGE = "message";
        private const string STATUS = "status";

		[SetUp]
		public void SetUp()
        {
			_testFixture = new Fixture();
			_contextContainer = new Mock<IContextContainer>();
			_serviceProxyFactory = new Mock<IServicesProxyFactory>();
			_objectManager = new Mock<IObjectManager>();
			_instanceSettingManager = new Mock<IInstanceSettingManager>();
			_workspaceManager = new Mock<IWorkspaceManager>();
			_contextContainer.Setup(x => x.ServicesProxyFactory).Returns(_serviceProxyFactory.Object);
			_serviceProxyFactory.Setup(x => x.CreateServiceProxy<IObjectManager>()).Returns(_objectManager.Object);
			_contextContainer.Setup(x => x.InstanceSettingManager).Returns(_instanceSettingManager.Object);
			_workspaceId = _testFixture.Create<int>();
            _imagingSetSchedulerId = _testFixture.Create<int>();
			_instance = new ObjectManagerHelper();
			_lastRun = _testFixture.Create<DateTime>();
			_nextRun = _testFixture.Create<DateTime>();
        }

		[Test]
		public async Task RetrieveAllImagingSetSchedulesNotWaitingAsync_GoldFlow()
		{
			//Arrange
			int batchsize = DEFAULT_BATCH_SIZE;
			_instanceSettingManager
				.Setup(x => x.GetIntegerValueAsync(SECTION, INSTANCE_SETTING_NAME, DEFAULT_BATCH_SIZE))
				.ReturnsAsync(batchsize);

			string condition = "(((NOT 'Status' ISSET) OR NOT ('Status' IN ['waiting'])))";

			QueryResult objectManagerResult = CreateObjectManagerResult();

			_objectManager.Setup(x => x.QueryAsync(
				It.Is<int>(i => i == _workspaceId),
				It.Is<QueryRequest>(q => q.Condition.Equals(condition)),
				It.Is<int>(j => j == 1),
				It.Is<int>(k => k == DEFAULT_BATCH_SIZE)
			)).ReturnsAsync(objectManagerResult);
			//Act
			List<RelativityObject> res = await _instance.RetrieveAllImagingSetSchedulesNotWaitingAsync(_workspaceId, _contextContainer.Object)
				.ConfigureAwait(false);
			//Assert
			Assert.True(res.SequenceEqual(objectManagerResult.Objects));
		}

        [Test]
        public async Task RetrieveSingleImagingSetSchedule_GoldFlow()
        {
			//Arrange
            int batchsize = DEFAULT_BATCH_SIZE;
            _instanceSettingManager
                .Setup(x => x.GetIntegerValueAsync(SECTION, INSTANCE_SETTING_NAME, DEFAULT_BATCH_SIZE))
                .ReturnsAsync(batchsize);

            string condition = $"(('Artifact ID' == {_imagingSetSchedulerId}))";

            QueryResult objectManagerResult = CreateObjectManagerResult();

            _objectManager.Setup(x => x.QueryAsync(
                It.Is<int>(i => i == _workspaceId),
                It.Is<QueryRequest>(q => q.Condition.Equals(condition)),
                It.Is<int>(j => j == 1),
                It.Is<int>(k => k == DEFAULT_BATCH_SIZE)
            )).ReturnsAsync(objectManagerResult);

			//Act
            RelativityObject res = await _instance.RetrieveSingleImagingSetScheduler(_workspaceId, _contextContainer.Object, _imagingSetSchedulerId)
                .ConfigureAwait(false);
			
			//Assert
            Assert.True(res.Equals(objectManagerResult.Objects[0]));
        }

        [Test]
        public async Task UpdateImagingSetScheduler_GoldFlow()
        {
	        //Arrange
	        MassUpdateResult objectManagerMassUpdateResult = CreateObjectManagerMassUpdateResult();
	        string condition = $"(('Artifact ID' == {_imagingSetSchedulerId}))";

	        _objectManager.Setup(x => x.UpdateAsync(It.Is<int>(i => i == _workspaceId), It.Is<MassUpdateByCriteriaRequest>(m=>m.ObjectIdentificationCriteria.Condition == condition))).ReturnsAsync(objectManagerMassUpdateResult);

			//Act
			var res = await _instance.UpdateImagingSetScheduler(_workspaceId, _contextContainer.Object,
				_imagingSetSchedulerId, _lastRun, _nextRun, MESSAGE, STATUS).ConfigureAwait(false);

			//Assert
			Assert.True(res.Success.Equals(objectManagerMassUpdateResult.Success));
        }

        [Test]
        public async Task DoesWorkspaceExists_GoldFlow()
        {
			//Arrange
			_serviceProxyFactory.Setup(x => x.CreateServiceProxy<IWorkspaceManager>()).Returns(_workspaceManager.Object);

			WorkspaceResponse response = CreateWorkspaceResponse();

			_workspaceManager.Setup(x => x.ReadAsync(
				It.Is<int>(i => i == _workspaceId),
				It.Is<bool>(y=> y == false),
				It.Is<bool>(z => z == false))).ReturnsAsync(response);

			//Act
			bool res = await _instance.DoesWorkspaceExists(_workspaceId, _contextContainer.Object).ConfigureAwait(false);

			//Assert
			Assert.True(res.Equals(true));
        }

        [Test]
        public async Task DoesWorkspaceExists_NotFound()
        {
	        //Arrange
	        _serviceProxyFactory.Setup(x => x.CreateServiceProxy<IWorkspaceManager>()).Returns(_workspaceManager.Object);

	        _workspaceManager.Setup(x => x.ReadAsync(
		        It.Is<int>(i => i == _workspaceId),
		        It.Is<bool>(y => y == false),
		        It.Is<bool>(z => z == false))).ThrowsAsync(new NotFoundException());

	        //Act
	        bool res = await _instance.DoesWorkspaceExists(_workspaceId, _contextContainer.Object).ConfigureAwait(false);

	        //Assert
	        Assert.True(res.Equals(false));
        }

		private QueryResult CreateObjectManagerResult()
        {
            QueryResult result = new QueryResult();
            result.TotalCount = 1;
            result.Objects.Add(new RelativityObject
            {
                ArtifactID = _testFixture.Create<int>()
            });
            return result;
        }

        private MassUpdateResult CreateObjectManagerMassUpdateResult()
        {
	        MassUpdateResult result = new MassUpdateResult();
	        result.TotalObjectsUpdated = 1;
	        result.Success = true;
	        result.Message = "message";
	        return result;
        }

        private WorkspaceResponse CreateWorkspaceResponse()
        {
	        WorkspaceResponse response = new WorkspaceResponse();
	        response.Keywords = "placeholder";
	        response.Notes = "placeholder";
	        return response;
        }
    }
}
