﻿
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cider_x64
{
    internal class AppRestarter
    {
        internal interface IDispatcherAccessor
        {
            void BeginInvoke(Action action, DispatcherPriority priority);
        }

        DispatcherAccessor m_DispatcherAccessor = new DispatcherAccessor();
        class DispatcherAccessor : IDispatcherAccessor
        {
            Control m_GuiThreadDispatcherObject;
            public DispatcherAccessor()
            {
                m_GuiThreadDispatcherObject = new Control();
            }
            public void BeginInvoke(Action action, DispatcherPriority priority)
            {
                m_GuiThreadDispatcherObject.Dispatcher.BeginInvoke(action, priority);
            }
        }

        public void Restart()
        {
            // shutting down the Application object can only be done on the tread which created it
            Action runOnGuiThread = new Action(() =>
            {
                string currentProcessBinary = getCurrentProcessAssemblyLocation();
                startProcess(currentProcessBinary, null);

                shutdownApplication();
            });
            getDispatcherAccessor().BeginInvoke(runOnGuiThread, DispatcherPriority.Normal);
        }

        protected virtual IDispatcherAccessor getDispatcherAccessor()
        {
            return m_DispatcherAccessor;
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
