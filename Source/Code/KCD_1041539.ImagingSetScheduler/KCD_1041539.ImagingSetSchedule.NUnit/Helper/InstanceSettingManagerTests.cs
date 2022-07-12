using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoFixture;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Helper;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
    [TestFixture]
    public class InstanceSettingManagerTests
    {
        private const int DEFAULT_BATCH_SIZE = 1000;
        private const string SECTION = "Relativity.Imaging";
        private const string INSTANCE_SETTING_NAME = "ImagingSetSchedulerBatchSize";
        private InstanceSettingManager _instanceSettingManager; 
        private Mock<IInstanceSettingsBundle> _instanceSettingBundle;

        [SetUp]
        public void SetUp()
        {
            _instanceSettingBundle = new Mock<IInstanceSettingsBundle>();
            _instanceSettingManager = new InstanceSettingManager(_instanceSettingBundle.Object);
        }

        [Test]
        public async Task GetIntAsync_Test()
        {
            //Arrange
            int batchSize = DEFAULT_BATCH_SIZE;
            int expectedResult = 1; 

            _instanceSettingBundle.Setup(x => x.GetIntAsync(SECTION, INSTANCE_SETTING_NAME)).ReturnsAsync(expectedResult);

            //Act
            
            int result = await _instanceSettingManager.GetIntegerValueAsync(SECTION, INSTANCE_SETTING_NAME, batchSize).ConfigureAwait(false);

            //Assert
            Assert.True(result.Equals(expectedResult));
        }

        [Test]
        public async Task DefaultInstanceValue_Test()
        {
            //Arrange
            int expectedResult = 1000;

            //Act
            int result = await _instanceSettingManager.GetIntegerValueAsync(SECTION, INSTANCE_SETTING_NAME, DEFAULT_BATCH_SIZE)
                .ConfigureAwait(false);

            //Assert
            Assert.True(result.Equals(expectedResult));
        }
    }
}