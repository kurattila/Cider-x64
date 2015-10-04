using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;

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
        public void Restart_WillShutdownThroughDispatcher_Always()
        {
            var restarter = new Fake_AppRestarter();

            restarter.Restart();

            Assert.IsTrue(restarter.CalledShutdownApplicationThroughBeginInvoke);
        }
    }

    class Fake_AppRestarter : AppRestarter
                            , AppRestarter.IDispatcherAccessor
    {
        bool m_InsideBeginInvoke;
        public List<Delegate> BeginInvokedActions = new List<Delegate>();
        public void BeginInvoke(Action action, DispatcherPriority priority)
        {
            m_InsideBeginInvoke = true;
            action();
            m_InsideBeginInvoke = false;
        }

        protected override IDispatcherAccessor getDispatcherAccessor()
        {
            return this;
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

        public bool CalledShutdownApplicationThroughBeginInvoke = false;
        public bool CalledShutdownApplication = false;
        protected override void shutdownApplication()
        {
            CalledShutdownApplicationThroughBeginInvoke = m_InsideBeginInvoke;
            CalledShutdownApplication = true;
        }
    }
}
