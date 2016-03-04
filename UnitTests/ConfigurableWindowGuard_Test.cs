using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class ConfigurableWindowGuard_Test
    {
        [TestMethod]
        public void TurningOnTopMostFlag_WillSetEachWindowAsTopMost_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            bool win1TopMost = false;
            bool win2TopMost = false;
            win1.Setup(window => window.SetAlwaysOnTop(true)).Callback(() => win1TopMost = true);
            win2.Setup(window => window.SetAlwaysOnTop(true)).Callback(() => win2TopMost = true);
            mainVM.IsTopMostMainWindow = false; // simulate transition of false => true
            configGuard.Init(mainVM);

            configGuard.RegisterConfigurableWindow(win1.Object, new Fake_WindowConfiguration("win1"));
            configGuard.RegisterConfigurableWindow(win2.Object, new Fake_WindowConfiguration("win2"));
            mainVM.IsTopMostMainWindow = true;

            Assert.IsTrue(win1TopMost);
            Assert.IsTrue(win2TopMost);
        }

        [TestMethod]
        public void TurningOffTopMostFlag_WillSetEachWindowAsTopMost_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            bool win1TopMost = true;
            bool win2TopMost = true;
            win1.Setup(window => window.SetAlwaysOnTop(false)).Callback(() => win1TopMost = false);
            win2.Setup(window => window.SetAlwaysOnTop(false)).Callback(() => win2TopMost = false);
            mainVM.IsTopMostMainWindow = true; // simulate transition of true => false
            configGuard.Init(mainVM);

            configGuard.RegisterConfigurableWindow(win1.Object, new Fake_WindowConfiguration("win1"));
            configGuard.RegisterConfigurableWindow(win2.Object, new Fake_WindowConfiguration("win2"));
            mainVM.IsTopMostMainWindow = false;

            Assert.IsFalse(win1TopMost);
            Assert.IsFalse(win2TopMost);
        }

        [TestMethod]
        public void PropertyChangeEventOfMainViewModel_WontTouchWindowTopMostFlags_WhenChangedPropertyIsUninteresting()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            bool win1TopMost = false;
            bool win2TopMost = false;
            win1.Setup(window => window.SetAlwaysOnTop(true)).Callback(() => win1TopMost = true);
            win2.Setup(window => window.SetAlwaysOnTop(true)).Callback(() => win2TopMost = true);
            configGuard.Init(mainVM);

            configGuard.RegisterConfigurableWindow(win1.Object, new Fake_WindowConfiguration("win1"));
            configGuard.RegisterConfigurableWindow(win2.Object, new Fake_WindowConfiguration("win2"));
            mainVM.IsManualRestartButtonShown = true;

            Assert.IsFalse(win1TopMost);
            Assert.IsFalse(win2TopMost);
        }

        [TestMethod]
        public void WindowInitialized_WillLoadWindowConfiguration_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            configGuard.Init(mainVM);
            var winConfig1 = new Mock<WindowConfiguration>("win1");
            var winConfig2 = new Mock<WindowConfiguration>("win2");
            configGuard.RegisterConfigurableWindow(win1.Object, winConfig1.Object);
            configGuard.RegisterConfigurableWindow(win2.Object, winConfig2.Object);

            win1.Raise(window => window.ConfigurableWindowInitialized += null, EventArgs.Empty);
            win2.Raise(window => window.ConfigurableWindowInitialized += null, EventArgs.Empty);

            winConfig1.Verify(config => config.LoadSettings());
            winConfig2.Verify(config => config.LoadSettings());
        }

        [TestMethod]
        public void WindowInitialized_WillPositionWindowAccordingToLoadedConfig_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            configGuard.Init(mainVM);
            var winConfig1 = new Fake_WindowConfiguration("win1");
            var winConfig2 = new Fake_WindowConfiguration("win2");
            var win1Rect = new Rect(10, 20, 100, 200);
            var win2Rect = new Rect(30, 40, 300, 400);
            winConfig1.ForcedSettingsToLoad["Left"] = (int)win1Rect.Left;
            winConfig2.ForcedSettingsToLoad["Left"] = (int)win2Rect.Left;
            winConfig1.ForcedSettingsToLoad["Top"] = (int)win1Rect.Top;
            winConfig2.ForcedSettingsToLoad["Top"] = (int)win2Rect.Top;
            winConfig1.ForcedSettingsToLoad["Width"] = (int)win1Rect.Width;
            winConfig2.ForcedSettingsToLoad["Width"] = (int)win2Rect.Width;
            winConfig1.ForcedSettingsToLoad["Height"] = (int)win1Rect.Height;
            winConfig2.ForcedSettingsToLoad["Height"] = (int)win2Rect.Height;
            winConfig1.ForcedSettingsToLoad["IsTopMostWindow"] = 0 /*false*/;
            winConfig2.ForcedSettingsToLoad["IsTopMostWindow"] = 0 /*false*/;
            configGuard.RegisterConfigurableWindow(win1.Object, winConfig1);
            configGuard.RegisterConfigurableWindow(win2.Object, winConfig2);

            win1.Raise(window => window.ConfigurableWindowInitialized += null, EventArgs.Empty);
            win2.Raise(window => window.ConfigurableWindowInitialized += null, EventArgs.Empty);

            win1.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Left   == win1Rect.Left)));
            win1.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Top    == win1Rect.Top)));
            win1.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Width  == win1Rect.Width)));
            win1.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Height == win1Rect.Height)));
            win1.Verify(window => window.SetAlwaysOnTop(false));
            win2.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Left   == win2Rect.Left)));
            win2.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Top    == win2Rect.Top)));
            win2.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Width  == win2Rect.Width)));
            win2.Verify(window => window.SetPlacement(It.Is<Rect>(r => r.Height == win2Rect.Height)));
            win2.Verify(window => window.SetAlwaysOnTop(false));
        }

        [TestMethod]
        public void WindowClosed_WillStoreWindowConfiguration_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            configGuard.Init(mainVM);
            var winConfig1 = new Mock<WindowConfiguration>("win1");
            var winConfig2 = new Mock<WindowConfiguration>("win2");
            configGuard.RegisterConfigurableWindow(win1.Object, winConfig1.Object);
            configGuard.RegisterConfigurableWindow(win2.Object, winConfig2.Object);

            win1.Raise(window => window.ConfigurableWindowClosed += null, EventArgs.Empty);
            win2.Raise(window => window.ConfigurableWindowClosed += null, EventArgs.Empty);

            winConfig1.Verify(config => config.SaveSettings());
            winConfig2.Verify(config => config.SaveSettings());
        }

        [TestMethod]
        public void WindowClosed_WillStoreCurrentWindowPositionToWindowConfig_Always()
        {
            var mainVM = new MainViewModel();
            var configGuard = new ConfigurableWindowGuard();
            var win1 = new Mock<IConfigurableWindow>();
            var win2 = new Mock<IConfigurableWindow>();
            configGuard.Init(mainVM);
            var winConfig1 = new Fake_WindowConfiguration("win1");
            var winConfig2 = new Fake_WindowConfiguration("win2");
            var win1Rect = new Rect(10, 20, 100, 200);
            var win2Rect = new Rect(30, 40, 300, 400);
            win1.Setup(window => window.GetPlacement()).Returns(win1Rect);
            win2.Setup(window => window.GetPlacement()).Returns(win2Rect);
            win1.Setup(window => window.GetAlwaysOnTop()).Returns(false);
            win2.Setup(window => window.GetAlwaysOnTop()).Returns(false);
            configGuard.RegisterConfigurableWindow(win1.Object, winConfig1);
            configGuard.RegisterConfigurableWindow(win2.Object, winConfig2);

            win1.Raise(window => window.ConfigurableWindowClosed += null, EventArgs.Empty);
            win2.Raise(window => window.ConfigurableWindowClosed += null, EventArgs.Empty);

            Assert.AreEqual((int)win1Rect.Left,   winConfig1.SettingsSaved["Left"]);
            Assert.AreEqual((int)win1Rect.Top,    winConfig1.SettingsSaved["Top"]);
            Assert.AreEqual((int)win1Rect.Width,  winConfig1.SettingsSaved["Width"]);
            Assert.AreEqual((int)win1Rect.Height, winConfig1.SettingsSaved["Height"]);
            Assert.AreEqual(0 /*false*/, winConfig1.SettingsSaved["IsTopMostWindow"]);
            Assert.AreEqual((int)win2Rect.Left,   winConfig2.SettingsSaved["Left"]);
            Assert.AreEqual((int)win2Rect.Top,    winConfig2.SettingsSaved["Top"]);
            Assert.AreEqual((int)win2Rect.Width,  winConfig2.SettingsSaved["Width"]);
            Assert.AreEqual((int)win2Rect.Height, winConfig2.SettingsSaved["Height"]);
            Assert.AreEqual(0 /*false*/, winConfig2.SettingsSaved["IsTopMostWindow"]);
        }
    }
}
