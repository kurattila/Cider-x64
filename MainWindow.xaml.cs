using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoaderConfiguration m_Project = new LoaderConfiguration();
        WindowConfiguration m_WindowConfig = new WindowConfiguration("MainWindow");

        LoaderFactory m_LoaderFactory = new LoaderFactory();
        public MainWindow()
        {
            Initialized += MainWindow_Initialized;
            Closed += MainWindow_Closed;

            InitializeComponent();

            this.Dispatcher.BeginInvoke(
                new Action(() => { showGuiPreview(); })
                , DispatcherPriority.SystemIdle);
        }

        void MainWindow_Initialized(object sender, EventArgs e)
        {
            m_Project.LoadSettings();
            m_WindowConfig.LoadSettings();
            if (m_WindowConfig.ValidSettings())
            {
                this.Left = m_WindowConfig.Left;
                this.Top = m_WindowConfig.Top;
                this.Width = m_WindowConfig.Width;
                this.Height = m_WindowConfig.Height;
            }

            try
            {
                _fsWatcher = new FileSystemWatcher(Path.GetDirectoryName(m_Project.AssemblyOfPreviewedGui));
                _fsWatcher.Changed += fsWatcher_Changed;
                _fsWatcher.EnableRaisingEvents = true;
            }
            catch (System.ArgumentException)
            {
                // Assembly path may be wrong
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                m_Loader.CloseWindow();
            }
            catch(System.Runtime.Remoting.RemotingException)
            {
                // preview window might have been closed already
            }

            m_WindowConfig.Left = (int)Left;
            m_WindowConfig.Top = (int)Top;
            m_WindowConfig.Width = (int)Width;
            m_WindowConfig.Height = (int)Height;
            m_WindowConfig.SaveSettings();

            m_Project.SaveSettings();
        }

        FileSystemWatcher _fsWatcher;

        ILoader m_Loader;
        void showGuiPreview()
        {
            var waitIndicator = new WaitIndicator();
            waitIndicator.BeginWaiting(Left, Top, ActualWidth, ActualHeight);

            string assemblyDirectory = Path.GetDirectoryName(m_Project.AssemblyOfPreviewedGui);
            if (!string.IsNullOrEmpty(assemblyDirectory))
                Directory.SetCurrentDirectory(assemblyDirectory);

            m_Loader = m_LoaderFactory.Create();

            // Assemblies referenced from XAML through the "pack://application" syntax need to be loaded
            foreach (string assemblyToPreload in m_Project.PreloadedAssemblies)
            {
                m_Loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, assemblyToPreload));
            }

            // Load XAML Dictionaries like "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml"
            m_Loader.AddMergedDictionary(m_Project.ResourceDictionaryToAdd);

            m_Loader.Show(m_Project.AssemblyOfPreviewedGui, m_Project.TypeOfPreviewedGui);

            waitIndicator.EndWaiting();
        }

        bool _restartPending = false;
        void fsWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            string assemblyDirectory = Path.GetDirectoryName(m_Project.AssemblyOfPreviewedGui);
            if (Path.GetDirectoryName(e.FullPath) == assemblyDirectory)
            {
                if (_restartPending)
                    return;
                _restartPending = true;

                _fsWatcher.EnableRaisingEvents = false;

                requestAppRestart(null, null);
            }
        }

        AppRestarter m_Restarter = new AppRestarter();
        virtual protected void requestAppRestart(object sender, RoutedEventArgs e)
        {
            m_Restarter.Restart();
        }
    }
}
