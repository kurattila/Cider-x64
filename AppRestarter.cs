
using System;
using System.Windows;
using System.Windows.Threading;

namespace Cider_x64
{
    internal class AppRestarter
    {
        public void Restart()
        {
            launchDelayedAction(new Action(() =>
            {
                string currentProcessBinary = getCurrentProcessAssemblyLocation();
                startProcess(currentProcessBinary, null);

                shutdownApplication();
            }));
        }

        protected virtual void launchDelayedAction(Action action)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Normal, null);
        }

        protected virtual string getCurrentProcessAssemblyLocation()
        {
            return Application.ResourceAssembly.Location;
        }

        protected virtual void startProcess(string fileName, string arguments)
        {
            System.Diagnostics.Process.Start(fileName, arguments);
        }

        protected virtual void shutdownApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
