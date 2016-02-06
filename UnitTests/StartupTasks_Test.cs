using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class DefaultStartupTask_Test
    {
        class Fake_DefaultStartupTask : DefaultStartupTask
        {
            public bool CalledDoWebRequest = false;
            protected override void doWebRequest(HttpWebRequest webRequest)
            {
                CalledDoWebRequest = true;
            }

            public string CreatedConnectionString;
            protected override string createConnectionString(string webServiceUri, string machine, string version)
            {
                CreatedConnectionString = base.createConnectionString(webServiceUri, machine, version);
                return CreatedConnectionString;
            }
        }

        [TestMethod]
        public void Run_WillMakeWebRequest_Always()
        {
            var task = new Fake_DefaultStartupTask();

            task.Run();

            Assert.IsTrue(task.CalledDoWebRequest);
        }

        [TestMethod]
        public void Run_WillMakeCorrectConnectionString_Always()
        {
            var task = new Fake_DefaultStartupTask();
            task.WebServiceUri = "http://testuri.com/dummyRequest.php";
            task.MachineName = "dummyMachineName";
            task.Version = "1.2.3.4";

            task.Run();

            Assert.AreEqual("http://testuri.com/dummyRequest.php?computer=dummyMachineName&version=1.2.3.4", task.CreatedConnectionString);
        }
    }

    [TestClass]
    public class MockWebRequestStartupTask_Test
    {
        [TestMethod]
        public void WillUseMockWebService_Always()
        {
            var task = new MockWebRequestStartupTask();

            Assert.AreEqual("http://ubytovanie-vm.hyperlink.sk/Cider-x64-MockWebRequest.php", task.WebServiceUri);
        }
    }
}
