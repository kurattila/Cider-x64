using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace Cider_x64.GuiTests.Screens
{
    public abstract class UITestBase : IDisposable
    {
        public Application Application { get; private set; }

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
                Application = Application.Attach(ciderProcess.Id);
            else
                Application = null;
        }

        public void SetFileIntoFileOpenDialog(TestStack.White.UIItems.WindowItems.Window fileOpenDialog, string filePathToSet)
        {
            var filenameEditBox = fileOpenDialog.Get(SearchCriteria.ByAutomationId("1148").AndControlType(typeof(TextBox), WindowsFramework.Win32)) as TextBox;
            filenameEditBox.Text = filePathToSet;

            var okButton = fileOpenDialog.Get(SearchCriteria.ByAutomationId("1").AndControlType(typeof(Button), WindowsFramework.Win32)) as Button;
            okButton.Click();
        }

        public void WaitUntilAppExited()
        {
            while (!Application.HasExited)
                System.Threading.Thread.Sleep(100);
            System.Threading.Thread.Sleep(100);
        }

        public Window ShowFileOpenDialog()
        {
            Window mainWindow = Application.GetWindow("Cider x64");
            var menubar = mainWindow.GetMenuBar(SearchCriteria.ByAutomationId("mainMenu"));
            var fileMenu = menubar.MenuItemBy(SearchCriteria.ByAutomationId("fileMenuItem"));
            var fileOpenMenuItem = fileMenu.ChildMenus.Find(SearchCriteria.ByAutomationId("fileOpenMenuItem"));

            fileOpenMenuItem.Click();

            var fileOpenDialog = Application.Find(title => title == "Open", InitializeOption.NoCache); // Application.GetWindow("Open");
            return fileOpenDialog;
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
