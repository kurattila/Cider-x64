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
            loaderConfig.AddMruItem(@"C:\one.dll");
            loaderConfig.AddMruItem(@"C:\two.dll");

            loaderConfig.SaveSettings();

            Assert.AreEqual(5, loaderConfig.SettingsSaved.Count);
            Assert.AreEqual("C:\\abc\\someModule.DLL", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_AssemblyFullPath]);
            Assert.AreEqual("SomeNamespace.SomeType", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_Type]);
            Assert.AreEqual("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_ToAddMergedDictionary]);
            var preloadedAssemblies = loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_PreloadedAssemblies] as string[];
            Assert.AreEqual("dependentAssemblyOne.dll", preloadedAssemblies[0]);
            Assert.AreEqual("dependentAssemblyTwo.dll", preloadedAssemblies[1]);
            var mruAssemblies = loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_MruAssembliesList] as string[];
            Assert.AreEqual(@"C:\one.dll", mruAssemblies[0]);
            Assert.AreEqual(@"C:\two.dll", mruAssemblies[1]);
        }

        [TestMethod]
        public void SaveSettings_WillSaveEmptyNonNullStrings_WhenNullStringsRequiredToBeSaved()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AssemblyOfPreviewedGui = null;
            loaderConfig.TypeOfPreviewedGui = null;
            loaderConfig.ResourceDictionaryToAdd = null;

            loaderConfig.SaveSettings();

            Assert.AreEqual(5, loaderConfig.SettingsSaved.Count);
            Assert.AreEqual("", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_AssemblyFullPath]);
            Assert.AreEqual("", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_Type]);
            Assert.AreEqual("", loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_ToAddMergedDictionary]);
            var preloadedAssemblies = loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_PreloadedAssemblies] as string[];
            Assert.AreEqual(0, preloadedAssemblies.Length);
            var mruAssemblies = loaderConfig.SettingsSaved[LoaderConfiguration.SettingsName_MruAssembliesList] as string[];
            Assert.AreEqual(0, mruAssemblies.Length);
        }

        [TestMethod]
        public void LoadSettings_WillLoadAllData_Always()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_AssemblyFullPath] = @"C:\abc\someModule.DLL";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_Type] = "SomeNamespace.SomeType";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_ToAddMergedDictionary] = "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml";
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_PreloadedAssemblies] = new string[] { "dependentAssemblyOne.dll", "dependentAssemblyTwo.dll" };
            loaderConfig.ForcedSettingsToLoad[LoaderConfiguration.SettingsName_MruAssembliesList] = new string[] { @"C:\one.dll", @"C:\two.dll" };

            loaderConfig.LoadSettings();

            Assert.AreEqual(@"C:\abc\someModule.DLL", loaderConfig.AssemblyOfPreviewedGui);
            Assert.AreEqual("SomeNamespace.SomeType", loaderConfig.TypeOfPreviewedGui);
            Assert.AreEqual("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml", loaderConfig.ResourceDictionaryToAdd);
            Assert.AreEqual("dependentAssemblyOne.dll", loaderConfig.PreloadedAssemblies[0]);
            Assert.AreEqual("dependentAssemblyTwo.dll", loaderConfig.PreloadedAssemblies[1]);
            Assert.AreEqual(@"C:\one.dll", loaderConfig.GetFileMenuItemsCollection()[1].Title); // skip [0] since it's just a Separator
            Assert.AreEqual(@"C:\two.dll", loaderConfig.GetFileMenuItemsCollection()[2].Title); // skip [0] since it's just a Separator
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

        [TestMethod]
        public void AddMruItem_WillCreateSingleEntry_WhenSameAssemblyAddedTwoTimes()
        {
            var loaderConfig = new Fake_LoaderConfiguration();

            loaderConfig.AddMruItem(@"C:\one.dll");
            loaderConfig.AddMruItem(@"C:\one.dll");

            var fileMenuItems = loaderConfig.GetFileMenuItemsCollection();

            Assert.AreEqual(2, fileMenuItems.Count); // 1st one is just the Separator
            Assert.AreEqual(@"C:\one.dll", fileMenuItems[1].Title);
        }

        [TestMethod]
        public void AddMruItem_WillMoveAssemblyToTop_WhenThatAssemblyNotAtTop()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AddMruItem(@"C:\one.dll");
            loaderConfig.AddMruItem(@"C:\two.dll");
            loaderConfig.AddMruItem(@"C:\three.dll");

            loaderConfig.AddMruItem(@"C:\two.dll");

            var fileMenuItems = loaderConfig.GetFileMenuItemsCollection();
            Assert.AreEqual(4, fileMenuItems.Count); // 1st one is just the Separator
            Assert.AreEqual(@"C:\two.dll", fileMenuItems[1].Title);
            Assert.AreEqual(@"C:\three.dll", fileMenuItems[2].Title);
            Assert.AreEqual(@"C:\one.dll", fileMenuItems[3].Title);
        }

        [TestMethod]
        public void GetFileMenuItemsCollection_ReturnsEmptyCollection_ByDefault()
        {
            var loaderConfig = new Fake_LoaderConfiguration();

            var fileMenuItems = loaderConfig.GetFileMenuItemsCollection();

            Assert.IsNotNull(fileMenuItems);
            Assert.AreEqual(0, fileMenuItems.Count);
        }

        [TestMethod]
        public void GetFileMenuItemsCollection_ReturnsSeparatorPlus3ItemsInReversedOrder_When3DifferentMruItemsAdded()
        {
            var loaderConfig = new Fake_LoaderConfiguration();
            loaderConfig.AddMruItem(@"C:\abc.dll");
            loaderConfig.AddMruItem(@"C:\abc2.dll");
            loaderConfig.AddMruItem(@"C:\abc3.dll");

            var fileMenuItems = loaderConfig.GetFileMenuItemsCollection();

            Assert.AreEqual(true, fileMenuItems[0].IsSeparator);
            Assert.AreEqual(false, fileMenuItems[1].IsSeparator);
            Assert.AreEqual(@"C:\abc3.dll", fileMenuItems[1].Title);
            Assert.AreEqual(@"C:\abc2.dll", fileMenuItems[2].Title);
            Assert.AreEqual(@"C:\abc.dll", fileMenuItems[3].Title);
            Assert.AreEqual(4, fileMenuItems.Count);
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
