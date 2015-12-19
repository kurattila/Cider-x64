using System;
using System.Linq;
using System.Windows;

namespace Cider_x64
{
    // Derived from 'MarshalByRefObject', for details see http://stackoverflow.com/questions/1277346/net-problem-with-raising-and-handling-events-using-appdomains,
    // especially the summary words:
    // "The rule of thumb must be that if you wish to handle events across an application domain boundary,
    //  both types--the one exposing the event and the one handling it--must extend MarshalByRefObject."
    class SwitcherOfLoadedType : MarshalByRefObject
    {
        ILoader m_Loader;
        public ILoader Loader
        {
            get { return m_Loader; }
            set
            {
                // Make sure to install only 1 event handler every time our Loader changes
                if (m_Loader != value)
                {
                    if (m_Loader != null)
                        m_Loader.PreviewWindowClosed -= (sender, args) => activateRow(null);

                    m_Loader = value;
                    m_Loader.PreviewWindowClosed += (sender, args) => activateRow(null);
                }
            }
        }

        public MainViewModel MainViewModel { get; set; }
        public LoaderConfiguration LoaderConfiguration { get; set; }
        public Window MessageBoxOwner { get; set; }

        virtual protected void showMessageBoxSeam(string message, string caption)
        {
            MessageBox.Show(MessageBoxOwner, message, caption);
        }

        public void ToggleType(string namespaceDotType, WaitIndicator waitIndicator = null)
        {
            var currentActiveRow = (from row in MainViewModel.ListOfSelectedAssemblyTypes
                                    where row.IsShown == true
                                    select row).FirstOrDefault();
            Loader.CloseWindow(); // will affect current active row through 'PreviewWindowClosed'
            if (currentActiveRow != null && currentActiveRow.NamespaceDotType == namespaceDotType)
            {
                LoaderConfiguration.TypeOfPreviewedGui = null;
                activateRow(null);
                return;
            }

            LoaderConfiguration.TypeOfPreviewedGui = namespaceDotType;
            activateRow(namespaceDotType);

            bool success = true;

            try
            {
                Loader.LoadType(namespaceDotType);
            }
            catch (MissingPreloadException e)
            {
                if (waitIndicator != null)
                    waitIndicator.EndWaiting(); // dark progress overlay shall not obscure the MessageBox
                showMessageBoxSeam(InnermostExceptionExtractor.GetInnermostMessage(e), MissingPreloadException.TitleTextOfAdvice);
                success = false;
            }
            catch (Exception e)
            {
                showMessageBoxSeam(InnermostExceptionExtractor.GetInnermostMessage(e), null);
                success = false;
            }

            if (success)
            {
                try
                {
                    Loader.Show();
                }
                catch(Exception e)
                {
                    showMessageBoxSeam(InnermostExceptionExtractor.GetInnermostMessage(e), null);
                    success = false;
                }
            }

            if (!success)
                activateRow(null);
        }

        private ViewModel.GuiTypeViewModel activateRow(System.String typeToActivate)
        {
            ViewModel.GuiTypeViewModel activeRow = null;
            if (MainViewModel != null)
            {
                foreach (var row in MainViewModel.ListOfSelectedAssemblyTypes)
                {
                    bool isActiveRow = row.NamespaceDotType == typeToActivate;
                    if (isActiveRow)
                    {
                        activeRow = row;
                        row.IsShown = true;
                    }
                    else
                    {
                        row.IsShown = false;
                    }
                }
            }

            return activeRow;
        }
    }
}
