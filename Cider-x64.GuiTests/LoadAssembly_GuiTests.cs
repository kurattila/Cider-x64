using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cider_x64.GuiTests.Screens;
using TestStack.White.Factory;
using System.IO;
using TestStack.White.ScreenObjects;
using System.Linq;
using Microsoft.Win32;

namespace Cider_x64.GuiTests
{
    [TestClass]
    public class LoadAssembly_GuiTests : UITestBase
    {
        [TestMethod]
        public void AppStartup_WontCrash_WhenRegistryEmpty()
        {
            StartApp();
            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);

            Assert.AreEqual(1, mainWindow.FileMenuChildItems.Count); // "Open" item only - for a clean installation
        }

        [TestMethod]
        public void OpenAssembly_WillRestartApp_Always()
        {
            StartApp();
            int processIdBeforeRestart = Application.Process.Id;
            string ciderx64ExePath = Application.Process.StartInfo.FileName;
            string sampleGuiDllPath = Path.Combine(Path.GetDirectoryName(ciderx64ExePath), @"Cider-x64.SampleGuiElements.dll");

            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            var fileOpenDialog = mainWindow.ShowFileOpenDialog();
            fileOpenDialog.FilePath = sampleGuiDllPath;
            fileOpenDialog.ClickOk();
            WaitUntilAppExited();

            ReconnectAfterRestart();
            int processIdAfterRestart = Application.Process.Id;
            Assert.AreNotEqual(processIdBeforeRestart, processIdAfterRestart);
        }

        [TestMethod]
        public void TouchingAnAssembly_WillRestartAppOnlyOnce_WhenTouchingTheBinaryHundredTimesInARow()
        {
            StartApp();
            string ciderx64ExePath = Application.Process.StartInfo.FileName;
            string sampleGuiDllPath = Path.Combine(Path.GetDirectoryName(ciderx64ExePath), @"Cider-x64.SampleGuiElements.dll");
            var mainWindow = ScreenRepository.Get<CiderX64Window>("Cider x64", InitializeOption.NoCache);
            var fileOpenDialog = mainWindow.ShowFileOpenDialog();
            fileOpenDialog.FilePath = sampleGuiDllPath;
            fileOpenDialog.ClickOk();
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
