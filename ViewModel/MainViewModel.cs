﻿using Cider_x64.Helpers;
using Cider_x64.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Cider_x64
{
    public class MainViewModel : Helpers.ObservableBase
    {
        public MainViewModel()
        {
            ListOfSelectedAssemblyTypes.CollectionChanged += ListOfSelectedAssemblyTypes_CollectionChanged;
        }

        private void ListOfSelectedAssemblyTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("TextualInfoForAssemblyTypes");
        }

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
                this.NotifyPropertyChanged("TextualInfoForAssemblyTypes");
            }
        }
        private String m_SelectedAssembly;
        #endregion

        #region SelectedTypeOfPreview
        /// <summary>
        ///  SelectedTypeOfPreview
        /// </summary>
        /// <returns>current SelectedTypeOfPreview</returns>
        public GuiTypeViewModel SelectedTypeOfPreview
        {
            get { return m_SelectedTypeOfPreview; }
            set
            {
                m_SelectedTypeOfPreview = value;
                this.NotifyPropertyChanged("SelectedTypeOfPreview");
            }
        }
        private GuiTypeViewModel m_SelectedTypeOfPreview;
        #endregion

        #region ListOfSelectedAssemblyTypes
        /// <summary>
        ///  ListOfSelectedAssemblyTypes
        /// </summary>
        /// <returns>current ListOfSelectedAssemblyTypes</returns>
        public ObservableCollection<GuiTypeViewModel> ListOfSelectedAssemblyTypes
        {
            get { return m_ListOfSelectedAssemblyTypes; }
            set
            {
                if (value != m_ListOfSelectedAssemblyTypes)
                {
                    m_ListOfSelectedAssemblyTypes.CollectionChanged -= ListOfSelectedAssemblyTypes_CollectionChanged;
                    m_ListOfSelectedAssemblyTypes = value;
                    m_ListOfSelectedAssemblyTypes.CollectionChanged += ListOfSelectedAssemblyTypes_CollectionChanged;
                    this.NotifyPropertyChanged("ListOfSelectedAssemblyTypes");
                    this.NotifyPropertyChanged("TextualInfoForAssemblyTypes");
                }
            }
        }
        private ObservableCollection<GuiTypeViewModel> m_ListOfSelectedAssemblyTypes = new ObservableCollection<GuiTypeViewModel>();
        #endregion

        public static string NoAssemblyLoadedYet = "No assembly loaded yet";
        public static string NoGuiTypesInAssembly = "No GUI types in assembly";

        public string TextualInfoForAssemblyTypes
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedAssembly))
                    return MainViewModel.NoAssemblyLoadedYet;

                if (ListOfSelectedAssemblyTypes.Count > 0)
                    return "";
                else
                    return MainViewModel.NoGuiTypesInAssembly;
            }
        }

        ObservableCollection<FileMenuItemViewModel> m_FileMenuItems;
        public ObservableCollection<FileMenuItemViewModel> FileMenuItems
        {
            get { return m_FileMenuItems; }
            set
            {
                if (m_FileMenuItems != value)
                {
                    m_FileMenuItems = value;
                    NotifyPropertyChanged("FileMenuItems");
                }
            }
        }

        RestartHandler m_RestartHandler;
        public void SetRestartHandler(RestartHandler restartHandler)
        {
            m_RestartHandler = restartHandler;
            m_RestartHandler.IsAutoRestartPossibleChanged += (sender, args) => refreshIsAutoRestartButtonShownState();
            refreshIsAutoRestartButtonShownState();
        }

        void refreshIsAutoRestartButtonShownState()
        {
            IsManualRestartButtonShown = !m_RestartHandler.IsAutoRestartPossible();
        }

        bool m_IsManualRestartButtonShown;
        public bool IsManualRestartButtonShown
        {
            get { return m_IsManualRestartButtonShown; }
            set
            {
                if( m_IsManualRestartButtonShown != value)
                {
                    m_IsManualRestartButtonShown = value;
                    NotifyPropertyChanged("IsManualRestartButtonShown");
                }
            }
        }

        bool m_IsTopMostMainWindow;
        public bool IsTopMostMainWindow
        {
            get { return m_IsTopMostMainWindow; }
            set
            {
                if (m_IsTopMostMainWindow != value)
                {
                    m_IsTopMostMainWindow = value;
                    NotifyPropertyChanged("IsTopMostMainWindow");
                }
            }
        }

        [ExcludeFromCodeCoverage]
        protected virtual GuiTypeViewModel createGuiTypeViewModelInstance()
        {
            return new GuiTypeViewModel();
        }

        public void InitWithGuiTypes(List<string> guiTypes)
        {
            ListOfSelectedAssemblyTypes.Clear();

            foreach(string namespaceDotType in guiTypes)
            {
                var vm = createGuiTypeViewModelInstance();
                vm.NamespaceDotType = namespaceDotType;
                vm.IsShown = false;
                vm.ShowCommand = new RelayCommand(onShowCommand);

                ListOfSelectedAssemblyTypes.Add(vm);
            }
        }

        void onShowCommand(object param)
        {
            //m_WaitIndicator.BeginWaiting(0, 0, 0, 0);
            //ChangeTypeCommand.Execute(param);

            //if (param is string)
            //{
            string namespaceDotType = param as string;
            var vmOfShownGuiType = (from vm in ListOfSelectedAssemblyTypes
                                    where vm.NamespaceDotType == namespaceDotType
                                    select vm).FirstOrDefault();
            if (vmOfShownGuiType != null)
            {
                m_WaitIndicator.BeginWaiting(
                      PlayButtonWaitIndicatorAppearance
                    , vmOfShownGuiType.CurrentHilitedShowButtonRect.Left
                    , vmOfShownGuiType.CurrentHilitedShowButtonRect.Top
                    , vmOfShownGuiType.CurrentHilitedShowButtonRect.Width
                    , vmOfShownGuiType.CurrentHilitedShowButtonRect.Height);

                ChangeTypeCommand.Execute(param);

                m_WaitIndicator.EndWaiting();
            }
            //}
        }

        public IWaitIndicatorAppearance PlayButtonWaitIndicatorAppearance { get; set; }

        WaitIndicator m_WaitIndicator;
        public void SetWaitIndicator(WaitIndicator waitIndicator)
        {
            m_WaitIndicator = waitIndicator;
        }

        #region Commands
        /// <summary>
        /// This command selects new assembly
        /// </summary>
        public ICommand ChangeAssemblyCommand { get; set; }
        /// <summary>
        /// This command reacts on change in selected type
        /// </summary>
        public ICommand ChangeTypeCommand { get; set; }
        /// <summary>
        /// Loads a different assembly
        /// </summary>
        public ICommand MruFileCommand { get; set; }
        #endregion //Commands
    }
}
