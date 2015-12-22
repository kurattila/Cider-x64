using System.Windows.Input;

namespace Cider_x64.ViewModel
{
    internal class GuiTypeViewModel : Helpers.ObservableBase
    {
        private string m_NamespaceDotType;
        public string NamespaceDotType
        {
            get { return m_NamespaceDotType; }
            set
            {
                if (value != m_NamespaceDotType)
                {
                    m_NamespaceDotType = value;
                    NotifyPropertyChanged("NamespaceDotType");
                }
            }
        }

        private ICommand m_ShowCommand;
        public ICommand ShowCommand
        {
            get { return m_ShowCommand; }
            set
            {
                if (value != m_ShowCommand)
                {
                    m_ShowCommand = value;
                    NotifyPropertyChanged("ShowCommand");
                }
            }
        }

        private bool m_IsShown;
        public bool IsShown
        {
            get { return m_IsShown; }
            set
            {
                if (value != m_IsShown)
                {
                    m_IsShown = value;
                    NotifyPropertyChanged("IsShown");
                }
            }
        }
    }
}
