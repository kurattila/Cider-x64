using System;
using System.Windows;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowConfiguration m_WindowConfig = new WindowConfiguration("MainWindow");

        LoaderFactory m_LoaderFactory = new LoaderFactory();
        public MainWindow()
        {
            Initialized += MainWindow_Initialized;
            Closed += MainWindow_Closed;

            InitializeComponent();

            _assemblyPath = getAppSettingsRegistrykey().GetValue("GuiPreview-AssemblyFullPath") as string;
            _typeName = getAppSettingsRegistrykey().GetValue("GuiPreview-Namespace.TypeName") as string;
            if (string.IsNullOrEmpty(_assemblyPath))
                getAppSettingsRegistrykey().SetValue("GuiPreview-AssemblyFullPath", "");
            if (string.IsNullOrEmpty(_typeName))
                getAppSettingsRegistrykey().SetValue("GuiPreview-Namespace.TypeName", "");

            try
            {
                _fsWatcher = new FileSystemWatcher(Path.GetDirectoryName(_assemblyPath));
                _fsWatcher.Changed += fsWatcher_Changed;
                _fsWatcher.EnableRaisingEvents = true;
            }
            catch (System.ArgumentException)
            {
                // Assembly path may be wrong
            }

            this.IsVisibleChanged += MainWindow_IsVisibleChanged;
        }

        string _appRegistryBranch = "Cider-x64";
        string _appVersion = "1.0.0";
        void MainWindow_Initialized(object sender, EventArgs e)
        {
            m_WindowConfig.LoadSettings();
            if (m_WindowConfig.ValidSettings())
            {
                this.Left = m_WindowConfig.Left;
                this.Top = m_WindowConfig.Top;
                this.Width = m_WindowConfig.Width;
                this.Height = m_WindowConfig.Height;
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            m_Loader.CloseWindow();

            m_WindowConfig.Left = (int)Left;
            m_WindowConfig.Top = (int)Top;
            m_WindowConfig.Width = (int)Width;
            m_WindowConfig.Height = (int)Height;
            m_WindowConfig.SaveSettings();
        }

        RegistryKey getAppSettingsRegistrykey()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey(_appRegistryBranch);
            key = key.OpenSubKey(_appRegistryBranch, true);

            key.CreateSubKey(_appVersion);
            key = key.OpenSubKey(_appVersion, true);

            return key;
        }

        string _assemblyPath; // full path to assembly.dll
        string _typeName; // string of "namespace.classname" of type to instantiate

        FileSystemWatcher _fsWatcher;

        ILoader m_Loader;
        void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                string assemblyDirectory = Path.GetDirectoryName(_assemblyPath);
                Directory.SetCurrentDirectory(assemblyDirectory);

                m_Loader = m_LoaderFactory.Create();

                // Assemblies referenced from XAML through the "pack://application" syntax need to be loaded
                string[] assembliesToPreload = getAppSettingsRegistrykey().GetValue("GuiPreview-ToPreloadAssemblies") as string[];
                foreach (string assemblyToPreload in assembliesToPreload)
                {
                    m_Loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, assemblyToPreload));
                }

                // Load XAML Dictionaries like "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml"
                string mergedDirectoryToAdd = getAppSettingsRegistrykey().GetValue("GuiPreview-ToAddMergedDictionary") as string;
                m_Loader.AddMergedDictionary(mergedDirectoryToAdd);

                m_Loader.Show(_assemblyPath, _typeName);
            }
        }

        bool _restartPending = false;
        void fsWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            string assemblyDirectory = Path.GetDirectoryName(_assemblyPath);
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
