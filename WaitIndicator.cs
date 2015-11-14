using System;
using System.Threading;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Inspired by https://strukachev.wordpress.com/2011/01/09/wpf-progress-window-in-separate-thread/
    /// </summary>
    internal class WaitIndicator : IDisposable
    {
        private Thread m_BackgroundGuiThread;
        private AutoResetEvent m_WaitWindowShownEvent = new AutoResetEvent(false);
        private AutoResetEvent m_WaitWindowClosedEvent = new AutoResetEvent(false);
        double m_Left = 0;
        double m_Top = 0;
        double m_Width = 0;
        double m_Height = 0;

        protected IWindow m_WaitWindow;
        public void BeginWaiting(double left, double top, double width, double height)
        {
            m_Left = left;
            m_Top = top;
            m_Width = width;
            m_Height = height;

            m_BackgroundGuiThread = new Thread(doBackgroundGuiThreadWork);
            m_BackgroundGuiThread.Name = "Ciderx64-BackgroundGuiThread";
            m_BackgroundGuiThread.SetApartmentState(ApartmentState.STA);
            m_BackgroundGuiThread.Start();
            m_WaitWindowShownEvent.WaitOne();
        }

        public void EndWaiting()
        {
            // Request the WaitWindow to close (will include a fade-out animation while closing, so it'll take some time)
            m_WaitWindow.DispatcherInstance.BeginInvoke(new Action(() =>
            {
                m_WaitWindow.Close(m_WaitWindowClosedEvent);
            })
            , DispatcherPriority.Normal);
            // Wait for that window to close correctly
            m_WaitWindowClosedEvent.WaitOne();



            // Request Dispatcher to stop its modal frame => thus the background thread will end correctly
            m_WaitWindow.DispatcherInstance.BeginInvoke(new Action(() =>
            {
                requestEndBackgroundThread();
            })
            , DispatcherPriority.Normal);
            // Wait for background thread to end correctly => no need to Abort() which is not recommended anyway
            m_BackgroundGuiThread.Join();
//             m_BackgroundGuiThread.Abort();
        }

        protected bool IsBackgroundGuiThreadRunning()
        {
            return m_BackgroundGuiThread.ThreadState == ThreadState.Running;
        }

        protected virtual void doBackgroundGuiThreadWork()
        {
            m_WaitWindow = createWindow();
            m_WaitWindow.Show(m_Left, m_Top, m_Width, m_Height);
            m_WaitWindowShownEvent.Set();

            Dispatcher dispatcher = m_WaitWindow.DispatcherInstance;
            m_ModalDispatcherFrame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(exitFrame), m_ModalDispatcherFrame);
            Dispatcher.PushFrame(m_ModalDispatcherFrame);
        }

        DispatcherFrame m_ModalDispatcherFrame;
        void requestEndBackgroundThread()
        {
            m_ModalDispatcherFrame.Continue = false;
        }

        object exitFrame(object frame)
        {
            return null;
        }

        protected virtual IWindow createWindow()
        {
            return new WaitWindow();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    m_WaitWindowShownEvent.Dispose();
                    m_WaitWindowClosedEvent.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WaitIndicator() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
