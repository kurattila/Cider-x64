using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cider_x64
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window, IWindow
    {
        WaitWindowViewModel m_ViewModel = new WaitWindowViewModel();
        public WaitWindow()
        {
            InitializeComponent();

            this.waitWindow.DataContext = m_ViewModel;

            //m_ViewModel.WaitWindowVisualState = "Inactive" + Helpers.VisualStateManager_Accessor.NoTransitionPostfix;
            //VisualStateManager.GoToState(this.waitWindow, "Inactive", false);
            background.Opacity = 0;
            progressCircle.Opacity = 0;

            Closed += WaitWindow_Closed;
        }

        private void WaitWindow_Closed(object sender, EventArgs e)
        {
            m_WaitWindowClosedEvent.Set();
        }

        public Dispatcher DispatcherInstance
        {
            get
            {
                return this.Dispatcher;
            }
        }

        AutoResetEvent m_WaitWindowClosedEvent;
        Timer m_CloseTimer; // do _not_ use DispatcherTimer in a background thread: sometimes it just won't fire when there are no GUI events associated with that background thread!
        public void Close(AutoResetEvent waitWindowClosedEvent)
        {
            m_WaitWindowClosedEvent = waitWindowClosedEvent;
            m_ViewModel.WaitWindowVisualState = "Inactive";

            Duration fadeDuration = (Duration)Resources["fadeDuration"];

            m_CloseTimer = new Timer(onCloseTimerTick, null, TimeSpan.Zero, fadeDuration.TimeSpan);
        }

        private void onCloseTimerTick(object state)
        {
            m_CloseTimer.Dispose();

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => base.Close()));
        }

        public void Show(IWaitIndicatorAppearance appearance, double left, double top, double width, double height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            m_ViewModel.Appearance = appearance; // determine "appearance" yet before Show()

            var wih = new System.Windows.Interop.WindowInteropHelper(this);
            wih.EnsureHandle();
            Win32.User32.SetWindowLongPtr(new HandleRef(wih, wih.Handle), Win32.User32.GWL_HWNDPARENT, appearance.OwnerHwnd);

            Show();

            m_ViewModel.WaitWindowVisualState = "Active";
        }
    }

    public class WaitWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IWaitIndicatorAppearance Appearance { get; set; }

        string m_WaitWindowVisualState;
        public string WaitWindowVisualState
        {
            get { return m_WaitWindowVisualState; }
            set
            {
                if (m_WaitWindowVisualState != value)
                {
                    m_WaitWindowVisualState = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("WaitWindowVisualState"));
                }
            }
        }

        public double CircleSize
        {
            get { return Appearance.CircleSize; }
        }

        public Brush Background
        {
            get { return Appearance.Background; }
        }
    }
}
