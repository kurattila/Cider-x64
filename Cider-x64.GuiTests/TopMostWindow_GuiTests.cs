using System;
using System.Windows.Automation;
using Cider_x64.GuiTests.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace Cider_x64.GuiTests
{
    [TestClass]
    public class TopMostWindow_GuiTests : UITestBase
    {
        private RegistryKey CiderMainKey;
        private RegistryKey CiderVersionKey;

        public TopMostWindow_GuiTests()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"software", true /*writable*/);
            RegistryUtilities.CopyKey(rk, RegistryUtilities.RegistryBackupKeyName, RegistryUtilities.RegistryKeyName);
            CiderMainKey = rk.OpenSubKey(RegistryUtilities.RegistryKeyName, true);
            string ciderVersion = CiderMainKey.GetSubKeyNames()[0];
            CiderVersionKey = CiderMainKey.OpenSubKey(ciderVersion, true);
        }

        [TestMethod]
        public void ApplicationStartup_WillMakeBothWindowsTopMost_WhenTopMostFlagSetForMainWindow()
        {
            var ciderMainWindowKey = CiderVersionKey.OpenSubKey("MainWindow", true);
            ciderMainWindowKey.SetValue("IsTopMostWindow", 1 /*true*/);
            StartApp();

            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            Assert.IsTrue(mainWindow.IsTopMostWindow());
        }

        [TestMethod]
        public void ApplicationStartup_WillLeaveBothWindowsNonTopMost_WhenTopMostFlagNotSetForMainWindow()
        {
            var ciderMainWindowKey = CiderVersionKey.OpenSubKey("MainWindow", true);
            ciderMainWindowKey.SetValue("IsTopMostWindow", 0 /*false*/);
            StartApp();

            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            Assert.IsFalse(mainWindow.IsTopMostWindow());
        }

        [TestMethod]
        public void InvokingTopMostMenuItemOfViewMenu_WillMakeBothWindowsNonTopMost_WhenUncheckingMenuItem()
        {
            var ciderMainWindowKey = CiderVersionKey.OpenSubKey("MainWindow", true);
            ciderMainWindowKey.SetValue("IsTopMostWindow", 1 /*true*/);
            ciderMainWindowKey.SetValue("GuiPreview-AssemblyFullPath", System.IO.Path.GetFullPath(GetBinariesPath() + "Cider-x64.SampleGuiElements.dll"));
            ciderMainWindowKey.SetValue("GuiPreview-Namespace.TypeName", "Ciderx64SampleGuiElements.SampleUserControlRed");
            StartApp();

            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            mainWindow.SetViewMenuTopMostFlag(false); // go from 'true' => 'false'

            var previewWindow = Application.GetWindow(SearchCriteria.ByText("Sample UserControl"), InitializeOption.NoCache);
            bool previewIsTopMost = (bool) previewWindow.AutomationElement.GetCurrentPropertyValue(WindowPatternIdentifiers.IsTopmostProperty);
            Assert.IsFalse(previewIsTopMost);
            Assert.IsFalse(mainWindow.IsTopMostWindow());
        }

        [TestMethod]
        public void InvokingTopMostMenuItemOfViewMenu_WillMakeBothWindowsTopMost_WhenCheckingMenuItem()
        {
            var ciderMainWindowKey = CiderVersionKey.OpenSubKey("MainWindow", true);
            ciderMainWindowKey.SetValue("IsTopMostWindow", 0 /*false*/);
            ciderMainWindowKey.SetValue("GuiPreview-AssemblyFullPath", System.IO.Path.GetFullPath(GetBinariesPath() + "Cider-x64.SampleGuiElements.dll"));
            ciderMainWindowKey.SetValue("GuiPreview-Namespace.TypeName", "Ciderx64SampleGuiElements.SampleUserControlRed");
            StartApp();

            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            mainWindow.SetViewMenuTopMostFlag(true); // go from 'false' => 'true'

            var previewWindow = Application.GetWindow(SearchCriteria.ByText("Sample UserControl"), InitializeOption.NoCache);
            bool previewIsTopMost = (bool) previewWindow.AutomationElement.GetCurrentPropertyValue(WindowPatternIdentifiers.IsTopmostProperty);
            Assert.IsTrue(previewIsTopMost);
            Assert.IsTrue(mainWindow.IsTopMostWindow());
        }
    }
}
