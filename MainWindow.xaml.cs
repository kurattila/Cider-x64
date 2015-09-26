using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            DesignerProperties.IsInDesignModeProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | FrameworkPropertyMetadataOptions.Inherits));
        }

        public MainWindow()
        {
            this.Initialized += MainWindow_Initialized;
            this.Closed += MainWindow_Closed;

            InitializeComponent();

            _assemblyPath = getAppSettingsRegistrykey().GetValue("GuiPreview-AssemblyFullPath") as string;
            _typeName = getAppSettingsRegistrykey().GetValue("GuiPreview-Namespace.TypeName") as string;
            if (string.IsNullOrEmpty(_assemblyPath))
                getAppSettingsRegistrykey().SetValue("GuiPreview-AssemblyFullPath", "");
            if (string.IsNullOrEmpty(_typeName))
                getAppSettingsRegistrykey().SetValue("GuiPreview-Namespace.TypeName", "");

            try
            {
                _fsWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(_assemblyPath));
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
        static readonly int _undefinedWindowPosition = -10000;
        void MainWindow_Initialized(object sender, EventArgs e)
        {
            int leftPos = 0;
            int topPos = 0;
            int width = 0;
            int height = 0;

            string leftRaw = getAppSettingsRegistrykey().GetValue("Left", MainWindow._undefinedWindowPosition) as string;
            if (leftRaw != null)
                leftPos = int.Parse(leftRaw);

            string topRaw = getAppSettingsRegistrykey().GetValue("Top", MainWindow._undefinedWindowPosition) as string;
            if (topRaw != null)
                topPos = int.Parse(topRaw);

            string widthRaw = getAppSettingsRegistrykey().GetValue("Width", MainWindow._undefinedWindowPosition) as string;
            if (widthRaw != null)
                width = int.Parse(widthRaw);

            string heightRaw = getAppSettingsRegistrykey().GetValue("Height", MainWindow._undefinedWindowPosition) as string;
            if (heightRaw != null)
                height = int.Parse(heightRaw);

            if (leftPos != MainWindow._undefinedWindowPosition
                && topPos != MainWindow._undefinedWindowPosition)
            {
                this.Left = leftPos;
                this.Top = topPos;
                if (width > 0)
                    this.Width = width;
                if (height > 0)
                    this.Height = height;
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            getAppSettingsRegistrykey().SetValue("Left", this.Left);
            getAppSettingsRegistrykey().SetValue("Top", this.Top);
            getAppSettingsRegistrykey().SetValue("Width", this.Width);
            getAppSettingsRegistrykey().SetValue("Height", this.Height);
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

        string getShadowCopyPath(string origiAssembly)
        {
            string path = System.IO.Path.GetDirectoryName(origiAssembly);
            string filenameNoExtension = System.IO.Path.GetFileNameWithoutExtension(origiAssembly);
            string fileExtension = System.IO.Path.GetExtension(origiAssembly);

            string shadowCopyPath = System.IO.Path.Combine(path, filenameNoExtension + " - Copy" + fileExtension);
            return shadowCopyPath;
        }

        System.IO.FileSystemWatcher _fsWatcher;

        void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;
            if (isVisible)
                reshowWindow(_assemblyPath, _typeName);
        }

        private void reshowWindow(string assemblyPath, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(typeName))
                return;

            try
            {
                File.Copy(assemblyPath, getShadowCopyPath(_assemblyPath), true);
            }
            catch (System.Exception)
            {
                this.Dispatcher.BeginInvoke(new Action(() => reshowWindow(assemblyPath, typeName)), DispatcherPriority.Normal, null);
                return;
            }

            // Assemblies referenced from XAML through the "pack://application" syntax need to be loaded
            string assemblyDirectory = System.IO.Path.GetDirectoryName(assemblyPath);
            Assembly.LoadFrom(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly1.dll"));
            Assembly.LoadFrom(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly2.dll"));
            Assembly.LoadFrom(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly3.dll"));
            Assembly.LoadFrom(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly4.dll"));

            ResourceDictionary xamlDictionaryToMerge = new ResourceDictionary();
            xamlDictionaryToMerge.Source = new Uri("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml");
            Application.Current.Resources.MergedDictionaries.Add(xamlDictionaryToMerge);

            Assembly assembly = Assembly.LoadFrom(getShadowCopyPath(_assemblyPath));
            Type typeToCreate = assembly.GetType(typeName);
            object instanceCreated = Activator.CreateInstance(typeToCreate);
            displayWpfGuiPreview(instanceCreated as Window);
            displayWpfGuiPreview(instanceCreated as UserControl);
        }

        private void displayWpfGuiPreview(Window instanceCreated)
        {
            if (instanceCreated == null)
                return;

            instanceCreated.Owner = this;
            instanceCreated.Left = this.Left;
            instanceCreated.Top = this.Top;
            instanceCreated.Show();
        }

        private void displayWpfGuiPreview(UserControl instanceCreated)
        {
            if (instanceCreated == null)
                return;

            this.root.Children.Add(instanceCreated);
        }

        void fsWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (e.FullPath == _assemblyPath)
            {
                _fsWatcher.EnableRaisingEvents = false;

                RestartButton_Click(null, null);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, null);
                Application.Current.Shutdown();
            })
                , DispatcherPriority.Normal, null);
        }
    }
}
