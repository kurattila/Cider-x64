namespace WpfGuiHelpers
{
    /// <summary>
    /// Provides a base class for ViewModels (see MVVM pattern)
    /// </summary>
    /// \brief        ObservableBase.NotifyPropertyChanged - notifies all its observers about a property change
    public class ObservableBase : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// INotifyPropertyChanged implementation
        /// </summary>
        public virtual event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///! \brief Makes any pending (deferred) PropertyChanged notifications to be sent immediately
        /// </summary>
        public static void FlushDeferredNotifications()
        {
            if (m_DeferringTimer != null)
            {
                m_DeferringTimer.Stop();
                safeFlushOfCurrentlyDeferred();

                // Only one instance of DispatcherTimer will exist at most; no need to cleanup before the application exits
                // m_DeferringTimer.Tick -= new System.EventHandler(onDeferringDelayTick);
                // m_DeferringTimer = null;
            }
        }

        /// <summary>
        ///! \brief Notify listeners that the specified property has changed
        ///! \param propertyName Name of the property which has changed
        /// </summary>
        protected void NotifyPropertyChanged(System.String propertyName)
        {
            System.Diagnostics.Debug.Assert(this != null);
            System.Diagnostics.Debug.Assert(propertyName != null);

            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///! \brief Notify listeners (but only after a delay) that the specified property has changed
        ///! \param propertyName Name of the property the change of which should be propagated with some delay
        /// </summary>
        protected void NotifyPropertyChangedDeferred(System.String propertyName)
        {
            if (m_DeferringTimer == null)
            {
                m_DeferringTimer = new System.Windows.Threading.DispatcherTimer();
                m_DeferringTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 400);
                m_DeferringTimer.Tick += new System.EventHandler(onDeferringDelayTick);
            }

            // Normally, we need just to extend the delay for the same property by restarting the timer.
            // But if it's a new request, then Flush the currently delayed property and delay the new one
            if (m_ViewModelOfDeferredChange != this || m_PropertyNameOfDeferredChange != propertyName)
            {
                if (m_DeferringTimer.IsEnabled)
                    safeFlushOfCurrentlyDeferred();

                m_ViewModelOfDeferredChange = this;
                m_PropertyNameOfDeferredChange = propertyName;
            }

            m_DeferringTimer.Stop();
            m_DeferringTimer.Start();
        }

        private static System.String m_PropertyNameOfDeferredChange;
        private static ObservableBase m_ViewModelOfDeferredChange;
        private static System.Windows.Threading.DispatcherTimer m_DeferringTimer;

        private static void onDeferringDelayTick(System.Object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.Assert(m_ViewModelOfDeferredChange != null);
            System.Diagnostics.Debug.Assert(m_PropertyNameOfDeferredChange != null);

            if (m_DeferringTimer.IsEnabled)
                safeFlushOfCurrentlyDeferred();

            m_ViewModelOfDeferredChange = null;
            m_PropertyNameOfDeferredChange = null;
            m_DeferringTimer.Stop();
        }

        private static void safeFlushOfCurrentlyDeferred()
        {
            if (m_ViewModelOfDeferredChange != null && m_PropertyNameOfDeferredChange != null)
            {
                m_ViewModelOfDeferredChange.NotifyPropertyChanged(m_PropertyNameOfDeferredChange);
                m_ViewModelOfDeferredChange = null;
                m_PropertyNameOfDeferredChange = null;
            }
        }
    }
};
