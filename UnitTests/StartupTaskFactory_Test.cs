using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class StartupTaskFactory_Test
    {
        [TestMethod]
        public void CreateCreateStartupTask_WillCreateDefaultTask_WhenArgsEmpty()
        {
            var startupTask = StartupTaskFactory.CreateStartupTask(new string[] { });

            Assert.IsNotNull(startupTask);
            Assert.IsTrue(startupTask is DefaultStartupTask);
        }

        [TestMethod]
        public void CreateCreateStartupTask_WillCreateNullTask_WhenNoVersionCheckArgsPassed()
        {
            var startupTask = StartupTaskFactory.CreateStartupTask(new string[] { StartupTaskFactory.NoCheckVersion });

            Assert.IsNotNull(startupTask);
            Assert.IsTrue(startupTask is NullStartupTask);
        }

        [TestMethod]
        public void CreateCreateStartupTask_WillCreateMockTask_WhenTestArgsPassed()
        {
            var startupTask = StartupTaskFactory.CreateStartupTask(new string[] { StartupTaskFactory.TestWebRequest });

            Assert.IsNotNull(startupTask);
            Assert.IsTrue(startupTask is MockWebRequestStartupTask);
        }
    }
}
