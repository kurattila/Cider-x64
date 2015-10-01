using System;
using System.Windows;
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

        FileSystemWatcher _fsWatcher;

        void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;
            if (isVisible)
            {

                ILoader loader = m_LoaderFactory.Create();

                // Assemblies referenced from XAML through the "pack://application" syntax need to be loaded
                //string assemblyDirectory = Path.GetDirectoryName(_assemblyPath);
                //loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly1.dll"));
                //loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly2.dll"));
                //loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly3.dll"));
                //loader.PreloadAssembly(System.IO.Path.Combine(assemblyDirectory, "SomeGuiAssembly4.dll"));

                //loader.AddMergedDictionary("pack://application:,,,/AnyAssembly;component/AnyPath/AnyResourceDictionary.xaml");

                loader.Show(_assemblyPath, _typeName);
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


    internal class LoaderFactory
    {
        ILoader m_Loader = null;
        AppDomain m_LoaderDomain = null;

        public ILoader Create()
        {
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ShadowCopyFiles = "true"; // not a boolean

            string selfAssemblyName = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;

            m_LoaderDomain = AppDomain.CreateDomain("GUI Preview Domain", null, adSetup);
            m_Loader = (ILoader) m_LoaderDomain.CreateInstanceFromAndUnwrap(selfAssemblyName, "Cider_x64.Loader");

            return m_Loader;
        }
    }
}
