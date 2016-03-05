using System;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Cider_x64.GuiTests.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;

namespace Cider_x64.GuiTests
{
	[TestClass]
	public class OwnerWindow_GuiTests : UITestBase
	{
        private RegistryKey CiderMainKey;
        private RegistryKey CiderVersionKey;

        public OwnerWindow_GuiTests()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"software", true /*writable*/);
            RegistryUtilities.CopyKey(rk, RegistryUtilities.RegistryBackupKeyName, RegistryUtilities.RegistryKeyName);
            CiderMainKey = rk.OpenSubKey(RegistryUtilities.RegistryKeyName, true);
            string ciderVersion = CiderMainKey.GetSubKeyNames()[0];
            CiderVersionKey = CiderMainKey.OpenSubKey(ciderVersion, true);
        }

        [TestMethod]
		public void PreviewWindow_IsOwnedByMainWindow_Always()
		{
            var ciderMainWindowKey = CiderVersionKey.OpenSubKey("MainWindow", true);
            ciderMainWindowKey.SetValue("GuiPreview-AssemblyFullPath", System.IO.Path.GetFullPath(GetBinariesPath() + "Cider-x64.SampleGuiElements.dll"));
            ciderMainWindowKey.SetValue("GuiPreview-Namespace.TypeName", "Ciderx64SampleGuiElements.SampleUserControlRed");

            StartApp();

            var mainWindowScreen = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            var previewWindowScreen = ScreenRepository.Get<PreviewWindow>(title => title == "Ciderx64SampleGuiElements.SampleUserControlRed", InitializeOption.NoCache);
            IntPtr mainWndHandle = mainWindowScreen.GetHwnd();
            IntPtr previewWndHandle = previewWindowScreen.GetHwnd();
            Assert.AreEqual(mainWndHandle, GetWindow(previewWndHandle, (uint)GetWindow_Cmd.GW_OWNER));
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
    }
}
