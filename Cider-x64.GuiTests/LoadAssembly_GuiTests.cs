using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cider_x64.GuiTests.Screens;
using TestStack.White.Configuration;
using TestStack.White.Factory;
using TestStack.White.Sessions;
using System.IO;
using System.Reflection;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.Finders;
using System.Linq;

namespace Cider_x64.GuiTests
{
    [TestClass]
    public class LoadAssembly_GuiTests : UITestBase
    {
        [TestMethod]
        public void AppStartup_WontCrash_WhenRegistryEmpty()
        {
            Window mainWindow = Application.GetWindow("Cider x64");
            var menubar = mainWindow.GetMenuBar(SearchCriteria.ByAutomationId("mainMenu"));
            var fileMenu = menubar.MenuItemBy(SearchCriteria.ByAutomationId("fileMenuItem"));

            Assert.AreEqual(1, fileMenu.ChildMenus.Count); // "Open" item only - for a clean installation
        }

        [TestMethod]
        public void OpenAssembly_WillRestartApp_Always()
        {
            int processIdBeforeRestart = Application.Process.Id;
            string ciderx64ExePath = Application.Process.StartInfo.FileName;
            string sampleGuiDllPath = Path.Combine(Path.GetDirectoryName(ciderx64ExePath), @"Cider-x64.SampleGuiElements.dll");

            var fileOpenDialog = ShowFileOpenDialog();
            SetFileIntoFileOpenDialog(fileOpenDialog, sampleGuiDllPath);
            WaitUntilAppExited();

            ReconnectAfterRestart();
            int processIdAfterRestart = Application.Process.Id;
            Assert.AreNotEqual(processIdBeforeRestart, processIdAfterRestart);
        }

        [TestMethod]
        public void TouchingAnAssembly_WillRestartAppOnlyOnce_WhenTouchingTheBinaryHundredTimesInARow()
        {
            string ciderx64ExePath = Application.Process.StartInfo.FileName;
            string sampleGuiDllPath = Path.Combine(Path.GetDirectoryName(ciderx64ExePath), @"Cider-x64.SampleGuiElements.dll");
            var fileOpenDialog = ShowFileOpenDialog();
            SetFileIntoFileOpenDialog(fileOpenDialog, sampleGuiDllPath);
            WaitUntilAppExited();
            ReconnectAfterRestart();
            int processIdBeforeRestart = Application.Process.Id;

            touchFileRepeatedly(sampleGuiDllPath, 2000);

            var ciderProcessInstances = from process in System.Diagnostics.Process.GetProcessesByName("Cider-x64")
                                        select process;
            int instancesCount = ciderProcessInstances.Count();
            foreach (var process in ciderProcessInstances)
                process.Kill();
            Assert.AreEqual(1, instancesCount);
        }

        void touchFileRepeatedly(string filePath, int repetitionsCount)
        {
            // How to do the same from a command prompt:
            // for /L %A in (1,1,200) do (echo %A & copy Wpf_AnimationD.dll /B+ ,,/Y)

            for (int i = 0; i < repetitionsCount; i++)
            {
                try
                {
                    System.IO.File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);
                }
                catch(IOException)
                {
                    ; // file could be "in use", but never mind, just go on and try the next time
                }
            }
        }
    }
}
