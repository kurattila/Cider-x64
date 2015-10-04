using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class AppRestarter_Test
    {
        [TestMethod]
        public void Restart_WillSpawnSameProcessAgain_Always()
        {
            var restarter = new Fake_AppRestarter();
            restarter.ForcedCurrentProcessAssemblyLocation = "dummyProcess.exe";

            restarter.Restart();

            Assert.AreEqual("dummyProcess.exe", restarter.ProcessStartedFilename);
            Assert.AreEqual(null, restarter.ProcessStartedArguments);
        }

        [TestMethod]
        public void Restart_WillShutdownProcessAfterLaunchingAnotherInstance_Always()
        {
            var restarter = new Fake_AppRestarter();
            restarter.ForcedCurrentProcessAssemblyLocation = "dummyProcess.exe";

            restarter.Restart();

            Assert.IsTrue(restarter.CalledShutdownApplication);
        }

        [TestMethod]
        public void Restart_WillDelayItsActions_Always()
        {
            var restarter = new Fake_AppRestarter();

            restarter.Restart();

            Assert.IsTrue(restarter.CalledLaunchDelayedAction);
        }
    }

    class Fake_AppRestarter : AppRestarter
    {
        public bool CalledLaunchDelayedAction = false;
        protected override void launchDelayedAction(Action action)
        {
            action();
            CalledLaunchDelayedAction = true;
        }

        public string ForcedCurrentProcessAssemblyLocation;
        protected override string getCurrentProcessAssemblyLocation()
        {
            return ForcedCurrentProcessAssemblyLocation;
        }

        public string ProcessStartedFilename;
        public string ProcessStartedArguments;
        protected override void startProcess(string fileName, string arguments)
        {
            if (!CalledShutdownApplication)
            {
                ProcessStartedFilename = fileName;
                ProcessStartedArguments = arguments;
            }
        }

        public bool CalledShutdownApplication = false;
        protected override void shutdownApplication()
        {
            CalledShutdownApplication = true;
        }
    }
}
