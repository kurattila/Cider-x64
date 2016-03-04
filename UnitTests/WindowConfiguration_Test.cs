using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class WindowConfiguration_Test
    {
        [TestMethod]
        public void SaveSettings_WillSaveWindowDimensions_Always()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.Left = 10;
            winConfig.Top = 20;
            winConfig.Width = 30;
            winConfig.Height = 40;
            winConfig.IsTopMostWindow = true;

            winConfig.SaveSettings();

            Assert.AreEqual(5, winConfig.SettingsSaved.Count);
            Assert.AreEqual(10, winConfig.SettingsSaved["Left"]);
            Assert.AreEqual(20, winConfig.SettingsSaved["Top"]);
            Assert.AreEqual(30, winConfig.SettingsSaved["Width"]);
            Assert.AreEqual(40, winConfig.SettingsSaved["Height"]);
            Assert.AreEqual(1 /*true*/, winConfig.SettingsSaved["IsTopMostWindow"]);
        }

        [TestMethod]
        public void LoadSettings_WillLoadWindowDimensions_Always()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.ForcedSettingsToLoad["Left"] = 10;
            winConfig.ForcedSettingsToLoad["Top"] = 20;
            winConfig.ForcedSettingsToLoad["Width"] = 30;
            winConfig.ForcedSettingsToLoad["Height"] = 40;
            winConfig.ForcedSettingsToLoad["IsTopMostWindow"] = 0 /*false*/;

            winConfig.LoadSettings();

            Assert.AreEqual(10, winConfig.Left);
            Assert.AreEqual(20, winConfig.Top);
            Assert.AreEqual(30, winConfig.Width);
            Assert.AreEqual(40, winConfig.Height);
        }

        [TestMethod]
        public void LoadSettings_WillLoadFromRegistryBranchOfSpecificCiderWindow_Always()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.ForcedSettingsToLoad["Left"] = 10;
            winConfig.ForcedSettingsToLoad["Top"] = 20;
            winConfig.ForcedSettingsToLoad["Width"] = 30;
            winConfig.ForcedSettingsToLoad["Height"] = 40;
            winConfig.ForcedSettingsToLoad["IsTopMostWindow"] = 0 /*false*/;

            winConfig.LoadSettings();

            Assert.AreEqual(@"HKEY_CURRENT_USER\Software\Cider-x64\1.0.0\winOne", winConfig.RegKeysLoadedFrom[0].RegPath);
        }

        [TestMethod]
        public void ValidSettings_ReturnsTrue_WhenAllSettingsMakeSense()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.Left = 10;
            winConfig.Top = 20;
            winConfig.Width = 30;
            winConfig.Height = 40;

            Assert.IsTrue(winConfig.ValidSettings());
        }

        [TestMethod]
        public void ValidSettings_ReturnsFalse_WhenAtLeastOneSettingUndefined()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.Left = 10;
            //winConfig.Top = 20;
            winConfig.Width = 30;
            winConfig.Height = 40;

            Assert.IsFalse(winConfig.ValidSettings());
        }

        [TestMethod]
        public void ValidSettings_ReturnsFalse_WhenWidthNegative()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.Left = 10;
            winConfig.Top = 20;
            winConfig.Width = -2147483648;
            winConfig.Height = 40;

            Assert.IsFalse(winConfig.ValidSettings());
        }

        [TestMethod]
        public void ValidSettings_ReturnsFalse_WhenHeightNegative()
        {
            var winConfig = new Fake_WindowConfiguration("winOne");
            winConfig.Left = 10;
            winConfig.Top = 20;
            winConfig.Width = 30;
            winConfig.Height = -2147483648;

            Assert.IsFalse(winConfig.ValidSettings());
        }
    }

    class Fake_WindowConfiguration : WindowConfiguration
    {
        public Fake_WindowConfiguration(string windowId) : base(windowId)
        { }

        protected override RegistryKeyWrapper getHKCU()
        {
            return new RegistryKeyWrapper()
            {
                RegistryKey = null, // Registry.CurrentUser -- isolate unit tests from Windows Registry
                RegPath = "HKEY_CURRENT_USER"
            };
        }

        public List<RegistryKeyWrapper> RegKeysSavedInto = new List<RegistryKeyWrapper>();
        public Dictionary<string, object> SettingsSaved = new Dictionary<string, object>();
        protected override void saveSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId, object value)
        {
            RegKeysSavedInto.Add(regKeyWrapper);
            SettingsSaved[settingId] = value;
        }

        public List<RegistryKeyWrapper> RegKeysLoadedFrom = new List<RegistryKeyWrapper>();
        public Dictionary<string, object> ForcedSettingsToLoad = new Dictionary<string, object>();
        protected override object loadSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId, object defaultValue)
        {
            RegKeysLoadedFrom.Add(regKeyWrapper);
            return ForcedSettingsToLoad[settingId];
        }
    }
}
