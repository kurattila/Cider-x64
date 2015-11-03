namespace Cider_x64.Helpers
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
    }
};
