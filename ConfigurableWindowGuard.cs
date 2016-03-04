using System;
using System.Collections.Generic;
using System.Windows;

namespace Cider_x64
{
    public interface IConfigurableWindow
    {
        void SetAlwaysOnTop(bool alwaysOnTop);
        void SetPlacement(Rect windowRect);
        bool GetAlwaysOnTop();
        Rect GetPlacement();

        event EventHandler ConfigurableWindowInitialized;
        event EventHandler ConfigurableWindowClosed;
    }

    public class ConfigurableWindowGuard : MarshalByRefObject
    {
        MainViewModel m_MainViewModel;
        public void Init(MainViewModel mainViewModel)
        {
            m_MainViewModel = mainViewModel;
            m_MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsTopMostMainWindow")
                return;

            foreach(var window in m_WindowsWithConfiguration.Keys)
            {
                window.SetAlwaysOnTop(m_MainViewModel.IsTopMostMainWindow);
            }
        }

        Dictionary<IConfigurableWindow, WindowConfiguration> m_WindowsWithConfiguration = new Dictionary<IConfigurableWindow, WindowConfiguration>();
        public void RegisterConfigurableWindow(IConfigurableWindow configurableWindow, WindowConfiguration windowConfig)
        {
            m_WindowsWithConfiguration[configurableWindow] = windowConfig;
            configurableWindow.ConfigurableWindowInitialized += ConfigurableWindow_Initialized;
            configurableWindow.ConfigurableWindowClosed += ConfigurableWindow_Closed;
        }

        private void ConfigurableWindow_Initialized(object sender, EventArgs args)
        {
            var configurableWindow = sender as IConfigurableWindow;
            var windowConfig = m_WindowsWithConfiguration[configurableWindow];

            windowConfig.LoadSettings();
            if (windowConfig.ValidSettings())
            {
                var rect = new Rect(windowConfig.Left, windowConfig.Top, windowConfig.Width, windowConfig.Height);
                configurableWindow.SetPlacement(rect);
            }
            configurableWindow.SetAlwaysOnTop(windowConfig.IsTopMostWindow);
        }

        private void ConfigurableWindow_Closed(object sender, EventArgs args)
        {
            var configurableWindow = sender as IConfigurableWindow;
            var windowConfig = m_WindowsWithConfiguration[configurableWindow];

            var currentWindowRect = configurableWindow.GetPlacement();
            windowConfig.Left = (int)currentWindowRect.Left;
            windowConfig.Top = (int)currentWindowRect.Top;
            windowConfig.Width = (int)currentWindowRect.Width;
            windowConfig.Height = (int)currentWindowRect.Height;
            windowConfig.IsTopMostWindow = configurableWindow.GetAlwaysOnTop();
            windowConfig.SaveSettings();
        }
    }
}
