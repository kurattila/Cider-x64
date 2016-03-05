using System;
using System.Windows.Input;
using Cider_x64.Helpers;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics.CodeAnalysis;

namespace Cider_x64.ViewModel
{
    public class GuiTypeViewModel : Helpers.ObservableBase
    {
        public GuiTypeViewModel()
        {
            UpdateCurrentShowButtonCoordsCommand = new RelayCommand(onUpdateHilitedShowButtonCoords);
        }

        Helpers.FrameworkElementToWin32CoordsConverter m_PlayButtonCoordsConverter = new FrameworkElementToWin32CoordsConverter();
        private void onUpdateHilitedShowButtonCoords(object fe)
        {
            var coordsConverter = GetFrameworkElementToWin32CoordsConverter();
            var rectRaw = coordsConverter.Convert(fe, typeof(Rect), null, System.Globalization.CultureInfo.InvariantCulture);
            CurrentHilitedShowButtonRect = (Rect)rectRaw;
            NotifyPropertyChanged("CurrentHilitedShowButtonRect");
        }

        public Rect CurrentHilitedShowButtonRect { get; private set; }

        /// <summary>
        /// Seam for getting a FrameworkElementToWin32CoordsConverter instance
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        virtual public IValueConverter GetFrameworkElementToWin32CoordsConverter()
        {
            return m_PlayButtonCoordsConverter;
        }

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
                    NotifyPropertyChanged("Namespace");
                    NotifyPropertyChanged("Class");
                }
            }
        }

        public string Namespace
        {
            get
            {
                string namespacePart = "";
                int lastDotPosition = m_NamespaceDotType.LastIndexOf('.');
                if (lastDotPosition != -1)
                    namespacePart = NamespaceDotType.Substring(0, lastDotPosition);
                return namespacePart;
            }
        }

        public string Class
        {
            get
            {
                string classPart = m_NamespaceDotType;
                int lastDotPosition = m_NamespaceDotType.LastIndexOf('.');
                if (lastDotPosition != -1)
                    classPart = NamespaceDotType.Substring(lastDotPosition + 1);
                return classPart;
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

        public ICommand UpdateCurrentShowButtonCoordsCommand { get; private set; }

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
