using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
                                           , IDisposable
    {
        LoaderConfiguration m_Project = new LoaderConfiguration();
        WindowConfiguration m_WindowConfig = new WindowConfiguration("MainWindow");

        LoaderFactory m_LoaderFactory = new LoaderFactory();
        public MainWindow()
        {
            Initialized += MainWindow_Initialized;
            Closed += MainWindow_Closed;
            StateChanged += MainWindow_StateChanged;

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MainViewDataContextChanged);
            this.DataContext = new MainViewModel();

            InitializeComponent();

            this.Dispatcher.BeginInvoke(
                new Action(() => { showGuiPreview(); })
                , DispatcherPriority.SystemIdle);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (m_Loader != null)
            {
                if (WindowState == WindowState.Minimized)
                    m_Loader.Hide();
                else
                    m_Loader.Show();
            }
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

            InitializeViewModel();
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

            m_Loader.LoadAssembly(m_Project.AssemblyOfPreviewedGui);
            m_Loader.LoadType(m_Project.TypeOfPreviewedGui);

            var asmTypes = m_Loader.GetLoadedAssemblyTypeNames();
            viewModel.ListOfSelectedAssemblyTypes = new ObservableCollection<string>(asmTypes);
            
            m_Loader.Show();

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
        void requestAppRestart(object sender, RoutedEventArgs e)
        {
            m_Restarter.Restart();
        }

        public void Dispose()
        {
            _fsWatcher.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        MainViewModel viewModel
        {
            get
            {
                Debug.Assert(this.DataContext as MainViewModel != null);
                return this.DataContext as MainViewModel;
            }
        }

        /// <summary>
        /// DataContextChanged event handler
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        void MainViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel vm = this.DataContext as MainViewModel;
            if (vm != null)
            {
                vm.ChangeAssemblyCommand = new Helpers.RelayCommand((param) => ChangeAssembly());
                vm.ChangedTypeCommand = new Helpers.RelayCommand((param) => ChangeType(param));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void InitializeViewModel()
        {
            if (m_Project.ValidSettings())
            {
                viewModel.SelectedAssembly = m_Project.AssemblyOfPreviewedGui;
                viewModel.SelectedTypeOfPreview = m_Project.TypeOfPreviewedGui;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ChangeAssembly()
        {
            // Create an instance of the open file dialog box.
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter options and filter index.
            fileDialog.Filter = "Dll Files (.dll)|*.dll|All Files (*.*)|*.*";
            fileDialog.FilterIndex = 1;

            fileDialog.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = fileDialog.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                viewModel.SelectedAssembly = fileDialog.FileName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void ChangeType(object type)
        {
            if (type is String)
            {
                Debug.WriteLine(type as String);
                
                m_Project.TypeOfPreviewedGui = type as String;
                viewModel.SelectedTypeOfPreview = m_Project.TypeOfPreviewedGui;

                m_Loader.CloseWindow();
                m_Loader.LoadType(type as string);
                m_Loader.Show();
                // m_Restarter.Restart();
            }
        }
    }
}
