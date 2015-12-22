using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cider_x64.ViewModel
{
    class FileMenuItemViewModel : Helpers.ObservableBase
    {
        string m_Title;
        public string Title
        {
            get { return m_Title; }
            set
            {
                if (value != m_Title)
                {
                    m_Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        bool m_IsSeparator;
        public bool IsSeparator
        {
            get { return m_IsSeparator; }
            set
            {
                if (value != m_IsSeparator)
                {
                    m_IsSeparator = value;
                    NotifyPropertyChanged("IsSeparator");
                }
            }
        }
    }
}
