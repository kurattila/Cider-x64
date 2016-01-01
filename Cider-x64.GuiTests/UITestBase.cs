using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TestStack.White;
using TestStack.White.ScreenObjects;

namespace Cider_x64.GuiTests.Screens
{
    public abstract class UITestBase : IDisposable
    {
        public Application Application { get; private set; }
        public ScreenRepository ScreenRepository { get; private set; }

        protected UITestBase()
        {
            backupRegistrySettings();

            var directoryName = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            directoryName = Uri.UnescapeDataString(directoryName); // e.g. otherwise, spaces in the path of our .EXE would be represented as '%20'
            directoryName = Path.GetFullPath(directoryName + @"\..");
            directoryName = Path.GetFullPath(directoryName + @"\..");
            directoryName = Path.GetFullPath(directoryName + @"\..");
            var markpadLocation = Path.GetFullPath(directoryName + @"\bin\x64\Debug\Cider-x64.exe");
            Application = Application.Launch(markpadLocation);
            ScreenRepository = new ScreenRepository(Application.ApplicationSession);
        }

        public void Dispose()
        {
            Application.Dispose();

            restoreRegistrySettingsFromBackup();
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
            Cider_x64.GuiTests.RegistryUtilities.RenameSubKey(rk, "Cider-x64", "Cider-x64.Backup");
        }

        void restoreRegistrySettingsFromBackup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"software", true /*writable*/);
            Cider_x64.GuiTests.RegistryUtilities.RenameSubKey(rk, "Cider-x64.Backup", "Cider-x64");
        }
    }
}
