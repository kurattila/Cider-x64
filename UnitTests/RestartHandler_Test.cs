using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class RestartHandler_Test
    {
        class Fake_AppRestarter : AppRestarter
        {
            public int CalledRestartsCount { get; private set; }
            public override void Restart()
            {
                CalledRestartsCount++;
            }
        }

        [TestMethod]
        public void RestartNow_WillMakeRestart_Always()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, null, dummyFsWatcherProxy);

            restartHandler.RestartNow();

            Assert.IsTrue(spyRestarter.CalledRestartsCount > 0);
        }

        [TestMethod]
        public void FsChange_WillMakeRestart_WhenDllInWatchedFolderChanged()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, @"C:\watchedFolder", dummyFsWatcherProxy);

            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");

            Assert.IsTrue(spyRestarter.CalledRestartsCount > 0);
        }

        [TestMethod]
        public void FsChange_WontMakeRestart_WhenDllOutsideWatchedFolderChanged()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, @"C:\watchedFolder", dummyFsWatcherProxy);

            restartHandler.OnFsChange(@"C:\otherFolder\abc.dll");

            Assert.AreEqual(0, spyRestarter.CalledRestartsCount);
        }

        [TestMethod]
        public void FsChange_WontMakeMultipleRestarts_WhenMultipleFsChangesDetected()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, @"C:\watchedFolder", dummyFsWatcherProxy);

            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");
            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");
            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");

            Assert.AreEqual(1, spyRestarter.CalledRestartsCount);
        }

        [TestMethod]
        public void FsChange_WontMakeRestart_WhenFsChangeDetectedDuringLoadingTime()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, @"C:\watchedFolder", dummyFsWatcherProxy);

            restartHandler.OnLoadingBegin();
            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");

            Assert.AreEqual(0, spyRestarter.CalledRestartsCount);
        }

        [TestMethod]
        public void FsChange_WontMakeRestart_WhenFsChangeDetectedBothDuringAndAfterLoadingTime()
        {
            var dummyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            var spyRestarter = new Fake_AppRestarter();
            restartHandler.Init(spyRestarter, @"C:\watchedFolder", dummyFsWatcherProxy);

            restartHandler.OnLoadingBegin();
            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");
            restartHandler.OnLoadingEnd();
            restartHandler.OnFsChange(@"C:\watchedFolder\abc.dll");

            Assert.AreEqual(0, spyRestarter.CalledRestartsCount);
        }

        class Fake_FsWatcherProxy : IFsWatcherProxy, IDisposable
        {
            public bool CalledDispose = false;

            public bool EnableRaisingEvents { get; set; }
            public string Path { get; set; }
            public void Dispose() { CalledDispose = true; }
            public void SetChangedHandler(FileSystemEventHandler handler) { m_ChangedEventHandler = handler; }
            FileSystemEventHandler m_ChangedEventHandler;

            public void ForceChangedEvent(string fullPath)
            {
                m_ChangedEventHandler(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, System.IO.Path.GetDirectoryName(fullPath), System.IO.Path.GetFileName(fullPath)));
            }
        }

        [TestMethod]
        public void Dispose_WillDisposeFsWatcher_Always()
        {
            var spyFsWatcherProxy = new Fake_FsWatcherProxy();

            using (RestartHandler restartHandler = new RestartHandler())
            {
                restartHandler.Init(null, null, spyFsWatcherProxy);
            }

            Assert.IsTrue(spyFsWatcherProxy.CalledDispose);
        }

        class Fake_RestartHandler : RestartHandler
        {
            public string CalledFsChangeFullPath;
            public override void OnFsChange(string fsChangeFullPath)
            {
                CalledFsChangeFullPath = fsChangeFullPath;
            }
        }

        [TestMethod]
        public void Init_WillSetWatchedFolderToFsWatcherProxy_Always()
        {
            var spyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            {
                restartHandler.Init(null, @"D:\someFolder", spyFsWatcherProxy);
            }

            Assert.AreEqual(@"D:\someFolder", spyFsWatcherProxy.Path);
        }

        [TestMethod]
        public void Init_WillEnableRaisingEventsOfFsWatcherProxy_WhenFolderPathToWatchNotEmpty()
        {
            var spyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            {
                restartHandler.Init(null, @"D:\someFolder", spyFsWatcherProxy);
            }

            Assert.IsTrue(spyFsWatcherProxy.EnableRaisingEvents);
        }

        [TestMethod]
        public void Init_WontEnableRaisingEventsOfFsWatcherProxy_WhenFolderPathToWatchEmpty()
        {
            var spyFsWatcherProxy = new Fake_FsWatcherProxy();
            RestartHandler restartHandler = new RestartHandler();
            {
                restartHandler.Init(null, "", spyFsWatcherProxy);
            }

            Assert.IsFalse(spyFsWatcherProxy.EnableRaisingEvents);
        }

        [TestMethod]
        public void FsWatcherEvent_WillCallOnFsChange_Always()
        {
            var stubFsWatcherProxy = new Fake_FsWatcherProxy();
            Fake_RestartHandler spyRestartHandler = new Fake_RestartHandler();
            {
                spyRestartHandler.Init(null, null, stubFsWatcherProxy);

                stubFsWatcherProxy.ForceChangedEvent(@"C:\dummyFolder\abc.dll");
            }

            Assert.AreEqual(@"C:\dummyFolder\abc.dll", spyRestartHandler.CalledFsChangeFullPath);
        }

        [TestMethod]
        public void IsAutoRestartPossible_ReturnsTrue_ByDefault()
        {
            bool autoRestartPossible = false;
            using (RestartHandler restartHandler = new RestartHandler())
            {
                autoRestartPossible = restartHandler.IsAutoRestartPossible();
            }

            Assert.IsTrue(autoRestartPossible);
        }

        [TestMethod]
        public void IsAutoRestartPossible_ReturnsFalse_WhenFsChangeDetectedDuringLoadingTime()
        {
            RestartHandler restartHandler = new RestartHandler();
            restartHandler.OnLoadingBegin();

            restartHandler.OnFsChange(@"dummyPath.dll");

            Assert.IsFalse(restartHandler.IsAutoRestartPossible());
            restartHandler.Dispose();
        }

        [TestMethod]
        public void IsAutoRestartPossible_ReturnsTrue_WhenFsChangeDetectedOnlyAfterLoadingTime()
        {
            RestartHandler restartHandler = new RestartHandler();
            restartHandler.OnLoadingBegin();
            restartHandler.OnLoadingEnd();

            restartHandler.OnFsChange(@"dummyPath.dll");

            Assert.IsTrue(restartHandler.IsAutoRestartPossible());
            restartHandler.Dispose();
        }

        [TestMethod]
        public void IsAutoRestartPossible_ReturnsFalse_WhenFsChangeDetectedBothDuringAndAfterLoadingTime()
        {
            RestartHandler restartHandler = new RestartHandler();

            restartHandler.OnLoadingBegin();
            restartHandler.OnFsChange(@"dummyPath.dll");
            restartHandler.OnLoadingEnd();
            restartHandler.OnFsChange(@"dummyPath.dll");

            Assert.IsFalse(restartHandler.IsAutoRestartPossible());
            restartHandler.Dispose();
        }

        [TestMethod]
        public void SettingIsAutoRestartPossible_WillFireIsAutoRestartPossibleChangedEvent_Always()
        {
            bool eventFired = false;
            RestartHandler restartHandler = new RestartHandler();
            restartHandler.IsAutoRestartPossibleChanged += (sender, args) => eventFired = true;

            restartHandler.OnLoadingBegin();
            restartHandler.OnFsChange(@"dummyPath.dll"); // shall fire IsAutoRestartPossibleChanged event

            Assert.IsTrue(eventFired);
            restartHandler.Dispose();
        }
    }
}
