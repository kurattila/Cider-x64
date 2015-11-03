using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Cider_x64
{
    class MainViewModel : Helpers.ObservableBase
    {

        #region SelectedAssembly
        /// <summary>
        ///  SelectedAssembly
        /// </summary>
        /// <returns>current SelectedAssembly</returns>
        public String SelectedAssembly
        {
            get { return m_SelectedAssembly; }
            set
            {
                m_SelectedAssembly = value;
                this.NotifyPropertyChanged("SelectedAssembly");
            }
        }
        private String m_SelectedAssembly;
        #endregion

        #region SelectedTypeOfPreview
        /// <summary>
        ///  SelectedTypeOfPreview
        /// </summary>
        /// <returns>current SelectedTypeOfPreview</returns>
        public String SelectedTypeOfPreview
        {
            get { return m_SelectedTypeOfPreview; }
            set
            {
                m_SelectedTypeOfPreview = value;
                this.NotifyPropertyChanged("SelectedTypeOfPreview");
            }
        }
        private String m_SelectedTypeOfPreview;
        #endregion

        #region ListOfSelectedAssemblyTypes
        /// <summary>
        ///  ListOfSelectedAssemblyTypes
        /// </summary>
        /// <returns>current ListOfSelectedAssemblyTypes</returns>
        public ObservableCollection<String> ListOfSelectedAssemblyTypes
        {
            get { return m_ListOfSelectedAssemblyTypes; }
            set
            {
                m_ListOfSelectedAssemblyTypes = value;
                this.NotifyPropertyChanged("ListOfSelectedAssemblyTypes");
            }
        }
        private ObservableCollection<String> m_ListOfSelectedAssemblyTypes;
        #endregion

        #region Commands
        /// <summary>
        /// This command selects new assembly
        /// </summary>
        public ICommand ChangeAssemblyCommand { get; set; }
        /// <summary>
        /// This command reacts on change in selected type
        /// </summary>
        public ICommand ChangedTypeCommand { get; set; }
        #endregion //Commands
    }
}
