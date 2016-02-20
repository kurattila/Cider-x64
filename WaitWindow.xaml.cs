using System;
using System.ComponentModel;
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
        public void Close(AutoResetEvent waitWindowClosedEvent)
        {
            m_WaitWindowClosedEvent = waitWindowClosedEvent;
            m_ViewModel.WaitWindowVisualState = "Inactive";

            Duration fadeDuration = (Duration)Resources["fadeDuration"];

            var timer = new DispatcherTimer();
            timer.Interval = fadeDuration.TimeSpan;
            timer.Tick += onCloseTimerTick;
            timer.Start();
        }

        private void onCloseTimerTick(object sender, EventArgs e)
        {
            base.Close();
        }

        public void Show(IWaitIndicatorAppearance appearance, double left, double top, double width, double height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            m_ViewModel.Appearance = appearance; // determine "appearance" yet before Show()
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
