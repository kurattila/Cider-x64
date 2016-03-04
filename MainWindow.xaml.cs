﻿using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
                                           , IConfigurableWindow
                                           , IDisposable
    {
        LoaderConfiguration m_Project = new LoaderConfiguration();

        ConfigurableWindowGuard m_ConfigurableWindowGuard = new ConfigurableWindowGuard();
        LoaderFactory m_LoaderFactory = new LoaderFactory();
        public MainWindow()
        {
            var vm = new MainViewModel();
            m_ConfigurableWindowGuard.Init(vm);
            m_ConfigurableWindowGuard.RegisterConfigurableWindow(this, new WindowConfiguration("MainWindow"));

            Initialized += MainWindow_Initialized;
            Closed += MainWindow_Closed;
            StateChanged += MainWindow_StateChanged;

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MainViewDataContextChanged);
            this.DataContext = vm;

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
            m_SwitcherOfLoadedType.LoaderConfiguration = m_Project;
            m_SwitcherOfLoadedType.MessageBoxOwner = this;

            if (ConfigurableWindowInitialized != null)
                ConfigurableWindowInitialized(this, EventArgs.Empty);

            m_RestartHandler.Init(new AppRestarter(), getDirectory(m_Project.AssemblyOfPreviewedGui));
            m_RestartHandler.OnLoadingBegin();

            InitializeViewModel();
        }

        string getDirectory(string fullPathOfAssembly)
        {
            string directory = "";

            try
            {
                directory = Path.GetDirectoryName(m_Project.AssemblyOfPreviewedGui);
            }
            catch(ArgumentException)
            {
                // Potentially bad project settings: e.g. assembly on that path doesn't exist
            }

            return directory;
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

            if (ConfigurableWindowClosed != null)
                ConfigurableWindowClosed(this, EventArgs.Empty);

            m_Project.SaveSettings();
        }

        RestartHandler m_RestartHandler = new RestartHandler();

        ILoader m_Loader;
        void showGuiPreview()
        {
            using (var waitIndicator = new WaitIndicator())
            {
                waitIndicator.BeginWaiting(new MainWindowWaitIndicatorAppearance(), Left, Top, ActualWidth, ActualHeight);

                m_Loader = m_LoaderFactory.Create();
                m_ConfigurableWindowGuard.RegisterConfigurableWindow(m_Loader.GetConfigurableWindow(), new WindowConfiguration("PreviewWindow"));

                if (!File.Exists(m_Project.AssemblyOfPreviewedGui))
                    return;

                string assemblyDirectory = Path.GetDirectoryName(m_Project.AssemblyOfPreviewedGui);
                if (!string.IsNullOrEmpty(assemblyDirectory))
                    Directory.SetCurrentDirectory(assemblyDirectory);

                // Assemblies referenced from XAML through the "pack://application" syntax need to be loaded
                foreach (string assemblyToPreload in m_Project.PreloadedAssemblies)
                {
                    m_Loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, assemblyToPreload));
                }

                // Load XAML Dictionaries like "pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml"
                m_Loader.AddMergedDictionary(m_Project.ResourceDictionaryToAdd);

                m_Loader.LoadAssembly(m_Project.AssemblyOfPreviewedGui);
                m_Project.AddMruItem(m_Project.AssemblyOfPreviewedGui);
                var asmTypes = m_Loader.GetLoadedAssemblyTypeNames();
                viewModel.InitWithGuiTypes(asmTypes);

                m_SwitcherOfLoadedType.Loader = m_Loader;
                ChangeType(m_Project.TypeOfPreviewedGui);

                m_RestartHandler.OnLoadingEnd();
            }
        }

        void requestAppRestart(object sender, RoutedEventArgs e)
        {
            m_RestartHandler.RestartNow();
        }

        public void Dispose()
        {
            m_RestartHandler.Dispose();
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
                vm.ChangeTypeCommand = new Helpers.RelayCommand((param) => ChangeType(param));
                vm.MruFileCommand = new Helpers.RelayCommand((param) => ChangeAssembly((param as ViewModel.FileMenuItemViewModel).Title));
                m_SwitcherOfLoadedType.MainViewModel = vm;
                vm.SetWaitIndicator(new WaitIndicator());
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
                viewModel.FileMenuItems = m_Project.GetFileMenuItemsCollection();
            }
            viewModel.SetRestartHandler(m_RestartHandler);
            viewModel.IsTopMostMainWindow = this.Topmost;
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
                ChangeAssembly(fileDialog.FileName);
        }

        private void ChangeAssembly(string assemblyPath)
        {
            m_Project.AssemblyOfPreviewedGui = assemblyPath;
            m_Project.TypeOfPreviewedGui = null;
            m_Project.SaveSettings();
            m_RestartHandler.RestartNow();
        }

        SwitcherOfLoadedType m_SwitcherOfLoadedType = new SwitcherOfLoadedType();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void ChangeType(object type, WaitIndicator waitIndicator = null)
        {
            m_SwitcherOfLoadedType.ToggleType(type as string, waitIndicator);
        }



        ///================================================================================
        /// IConfigurableWindow
        ///================================================================================
        [ExcludeFromCodeCoverage]
        public void SetAlwaysOnTop(bool alwaysOnTop)
        {
            this.Topmost = alwaysOnTop;
        }

        [ExcludeFromCodeCoverage]
        public void SetPlacement(Rect windowRect)
        {
            this.Left = windowRect.Left;
            this.Top = windowRect.Top;
            this.Width = windowRect.Width;
            this.Height = windowRect.Height;
        }

        [ExcludeFromCodeCoverage]
        public bool GetAlwaysOnTop()
        {
            return Topmost;
        }

        [ExcludeFromCodeCoverage]
        public Rect GetPlacement()
        {
            return new Rect(Left, Top, Width, Height);
        }

        public event EventHandler ConfigurableWindowInitialized;
        public event EventHandler ConfigurableWindowClosed;
    }
}
