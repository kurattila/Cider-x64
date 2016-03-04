using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TestStack.White;
using TestStack.White.ScreenObjects;
using System.Diagnostics;

namespace Cider_x64.GuiTests.Screens
{
    public abstract class UITestBase : IDisposable
    {
        public Application Application { get; private set; }
        public ScreenRepository ScreenRepository { get; private set; }

        protected UITestBase()
        {
            backupRegistrySettings();
        }

        public void StartApp()
        {
            var binsPath = GetBinariesPath();
            var ciderLocation = Path.GetFullPath(binsPath + "Cider-x64.exe");

            ProcessStartInfo info = new ProcessStartInfo(ciderLocation, "/nocheckversion"); // avoid making HTTP request on each UI test run
            Process ciderProcess = Process.Start(info);

            Application = Application.Attach(ciderProcess);
            ScreenRepository = new ScreenRepository(Application.ApplicationSession);
        }

        public void Dispose()
        {
            if (Application != null)
                Application.Dispose();

            restoreRegistrySettingsFromBackup();
        }

        public string GetBinariesPath()
        {
            var directoryName = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            directoryName = Uri.UnescapeDataString(directoryName); // e.g. otherwise, spaces in the path of our .EXE would be represented as '%20'
            directoryName = Path.GetFullPath(directoryName + @"\..");
            directoryName = Path.GetFullPath(directoryName + @"\..");
            directoryName = Path.GetFullPath(directoryName + @"\..");
            return Path.GetFullPath(directoryName + @"\bin\x64\Debug\");
        }

        public void ReconnectAfterRestart()
        {
            var ciderProcess = (from process in System.Diagnostics.Process.GetProcessesByName("Cider-x64")
                                select process).FirstOrDefault();
            if (ciderProcess != null)
            {
                Application = Application.Attach(ciderProcess.Id);
                ScreenRepository = new ScreenRepository(Application.ApplicationSession);
            }
            else
            {
                Application = null;
                ScreenRepository = null;
            }
        }

        public void WaitUntilAppExited()
        {
            while (!Application.HasExited)
                System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(100);
        }

        void backupRegistrySettings()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"software", true /*writable*/);
            Cider_x64.GuiTests.RegistryUtilities.RenameSubKey(rk, RegistryUtilities.RegistryKeyName, RegistryUtilities.RegistryBackupKeyName);
        }

        void restoreRegistrySettingsFromBackup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"software", true /*writable*/);
            Cider_x64.GuiTests.RegistryUtilities.RenameSubKey(rk, RegistryUtilities.RegistryBackupKeyName, RegistryUtilities.RegistryKeyName);
        }
    }
}
