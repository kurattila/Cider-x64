using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class LoaderConfiguration_Test
    {
        [TestMethod]
        public void SaveSettings_WillSaveAllData_Always()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AssemblyOfPreviewedGui = @"C:\abc\someModule.DLL";
            loaderConfig.TypeOfPreviewedGui = "SomeNamespace.SomeType";
            loaderConfig.ResourceDictionaryToAdd = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyOne.dll");
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyTwo.dll");

            loaderConfig.SaveSettings();

            Assert.AreEqual(4, loaderConfig.SettingsSaved.Count);
            Assert.AreEqual("C:\\abc\\someModule.DLL", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_AssemblyFullPath]);
            Assert.AreEqual("SomeNamespace.SomeType", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_Type]);
            Assert.AreEqual("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_ToAddMergedDictionary]);
            var preloadedAssemblies = loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_PreloadedAssemblies] as string[];
            Assert.AreEqual("dependentAssemblyOne.dll", preloadedAssemblies[0]);
            Assert.AreEqual("dependentAssemblyTwo.dll", preloadedAssemblies[1]);
        }

        [TestMethod]
        public void LoadSettings_WillLoadAllData_Always()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_AssemblyFullPath] = @"C:\abc\someModule.DLL";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_Type] = "SomeNamespace.SomeType";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_ToAddMergedDictionary] = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_PreloadedAssemblies] = new string[] { "dependentAssemblyOne.dll", "dependentAssemblyTwo.dll" };

            loaderConfig.LoadSettings();

            Assert.AreEqual(@"C:\abc\someModule.DLL", loaderConfig.AssemblyOfPreviewedGui);
            Assert.AreEqual("SomeNamespace.SomeType", loaderConfig.TypeOfPreviewedGui);
            Assert.AreEqual("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml", loaderConfig.ResourceDictionaryToAdd);
            Assert.AreEqual("dependentAssemblyOne.dll", loaderConfig.PreloadedAssemblies[0]);
            Assert.AreEqual("dependentAssemblyTwo.dll", loaderConfig.PreloadedAssemblies[1]);
        }

        [TestMethod]
        public void LoadSettings_WillLoadFromRegistryBranchOfSpecificCiderWindow_Always()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_AssemblyFullPath] = @"C:\abc\someModule.DLL";

            loaderConfig.LoadSettings();

            Assert.AreEqual(@"HKEY_CURRENT_USER\Software\Cider-x64\1.0.0", loaderConfig.RegKeysLoadedFrom[0].RegPath);
        }

        [TestMethod]
        public void ValidSettings_ReturnsTrue_WhenAllSettingsMakeSense()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AssemblyOfPreviewedGui = @"C:\abc\someModule.DLL";
            loaderConfig.ResourceDictionaryToAdd = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyOne.dll");
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyTwo.dll");

            Assert.IsTrue(loaderConfig.ValidSettings());
        }

        [TestMethod]
        public void ValidSettings_ReturnsFalse_WhenAssemblyOfPreviewedGuiNotSpecified()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
//             loaderConfig.AssemblyOfPreviewedGui = @"C:\abc\someModule.DLL";
            loaderConfig.ResourceDictionaryToAdd = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyOne.dll");
            loaderConfig.PreloadedAssemblies.Add("dependentAssemblyTwo.dll");

            Assert.IsFalse(loaderConfig.ValidSettings());
        }

        [TestMethod]
        public void ValidSettings_ReturnsTrue_WhenNotCriticalDataNotSpecified()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AssemblyOfPreviewedGui = @"C:\abc\someModule.DLL";
//             loaderConfig.ResourceDictionaryToAdd = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
//             loaderConfig.PreloadedAssemblies.Add("dependentAssemblyOne.dll");
//             loaderConfig.PreloadedAssemblies.Add("dependentAssemblyTwo.dll");

            Assert.IsTrue(loaderConfig.ValidSettings());
        }
    }

    class Fake_LoaderConfiguration : LoaderConfiguration
    {
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
            if (ForcedSettingsToLoad.ContainsKey(settingId))
                return ForcedSettingsToLoad[settingId];
            else
                return defaultValue;
        }
    }
}
