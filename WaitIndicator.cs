using System;
using System.Threading;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Inspired by https://strukachev.wordpress.com/2011/01/09/wpf-progress-window-in-separate-thread/
    /// </summary>
    internal class WaitIndicator
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
            m_WaitWindow.DispatcherInstance.BeginInvoke(new Action(() =>
            {
                m_WaitWindow.Close(m_WaitWindowClosedEvent);
            })
            , DispatcherPriority.Normal);

            m_WaitWindowClosedEvent.WaitOne();
            //m_BackgroundGuiThread.Join();
            m_BackgroundGuiThread.Abort();
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
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(exitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        object exitFrame(object frame)
        {
            return null;
        }

        protected virtual IWindow createWindow()
        {
            return new WaitWindow();
        }

        internal interface IWindow
        {
            void Show(double left, double top, double width, double height);
            void Close(AutoResetEvent waitWindowClosedEvent);
            Dispatcher DispatcherInstance { get; }
        }

        internal class WaitWindow : System.Windows.Window, IWindow
        {
            public WaitWindow()
            {
                this.Topmost = true;
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
                this.Content = new System.Windows.Controls.ProgressBar() { IsIndeterminate = true };
            }

            public void Close(AutoResetEvent waitWindowClosedEvent)
            {
                base.Close();
                waitWindowClosedEvent.Set();
            }

            public void Show(double left, double top, double width, double height)
            {
                this.Left = left;
                this.Top = top;
                this.Width = width;
                this.Height = height;
                Show();
            }

            public Dispatcher DispatcherInstance
            {
                get { return this.Dispatcher; }
            }
        }
    }
}
