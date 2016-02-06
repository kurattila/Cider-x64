using System;
using System.Threading.Tasks;
using System.Windows;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(InnermostExceptionExtractor.GetInnermostMessage(e.Exception));
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            IStartupTask startupTask = StartupTaskFactory.CreateStartupTask(e.Args);
            var startupBackgroundTask = new Action(() => startupTask.Run());
            Task.Run(startupBackgroundTask);
        }
    }
}
