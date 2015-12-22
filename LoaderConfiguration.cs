using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cider_x64
{
    internal class LoaderConfiguration : Configuration
    {
        public LoaderConfiguration()
        {
            PreloadedAssemblies = new List<string>();
        }

        public override void LoadSettings()
        {
            base.LoadSettings();

            AssemblyOfPreviewedGui = loadSingleSetting(getAppSettingsRegistrykey(), SettingsName_AssemblyFullPath, "") as string;
            TypeOfPreviewedGui = loadSingleSetting(getAppSettingsRegistrykey(), SettingsName_Type, "") as string;
            ResourceDictionaryToAdd = loadSingleSetting(getAppSettingsRegistrykey(), SettingsName_ToAddMergedDictionary, "") as string;
            string[] preloaded = loadSingleSetting(getAppSettingsRegistrykey(), SettingsName_PreloadedAssemblies, new string[] { }) as string[];
            PreloadedAssemblies = preloaded.ToList();

            string[] mruAssemblies = loadSingleSetting(getAppSettingsRegistrykey(), SettingsName_MruAssembliesList, new string[] { }) as string[];
            foreach (string mru in mruAssemblies.Reverse())
                AddMruItem(mru);
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            var regKey = getAppSettingsRegistrykey();
            saveSingleSetting(regKey, SettingsName_AssemblyFullPath, AssemblyOfPreviewedGui ?? "");
            saveSingleSetting(regKey, SettingsName_Type, TypeOfPreviewedGui ?? "");
            saveSingleSetting(regKey, SettingsName_ToAddMergedDictionary, ResourceDictionaryToAdd ?? "");
            saveSingleSetting(regKey, SettingsName_PreloadedAssemblies, PreloadedAssemblies.ToArray());

            var mruAssemblies = (from assembly in GetFileMenuItemsCollection()
                                 where assembly.IsSeparator == false
                                 select assembly.Title).Reverse();
            saveSingleSetting(regKey, SettingsName_MruAssembliesList, mruAssemblies.ToArray());
        }

        public override bool ValidSettings()
        {
            return string.IsNullOrEmpty(AssemblyOfPreviewedGui) == false;
        }

        public void AddMruItem(string assemblyFullPath)
        {
            if (m_FileMenuItemsCollection.Count == 0)
                m_FileMenuItemsCollection.Add(new ViewModel.FileMenuItemViewModel() { IsSeparator = true });

            var existingDuplicate = (from assembly in m_FileMenuItemsCollection
                                     where assembly.Title == assemblyFullPath
                                     select assembly).FirstOrDefault();
            if (existingDuplicate != null)
            {
                m_FileMenuItemsCollection.Remove(existingDuplicate);
            }

            // Insert new items at the top, but just below the Separator
            m_FileMenuItemsCollection.Insert(1, new ViewModel.FileMenuItemViewModel() { IsSeparator = false, Title = assemblyFullPath });
        }

        ObservableCollection<ViewModel.FileMenuItemViewModel> m_FileMenuItemsCollection = new ObservableCollection<ViewModel.FileMenuItemViewModel>();
        public ObservableCollection<ViewModel.FileMenuItemViewModel> GetFileMenuItemsCollection()
        {
            return m_FileMenuItemsCollection;
        }

        public string AssemblyOfPreviewedGui { get; set; }

        public string TypeOfPreviewedGui { get; set; }

        public string ResourceDictionaryToAdd { get; set; }

        public List<string> PreloadedAssemblies { get; set; }

        public static string SettingsName_AssemblyFullPath = "GuiPreview-AssemblyFullPath";
        public static string SettingsName_Type = "GuiPreview-Namespace.TypeName";
        public static string SettingsName_ToAddMergedDictionary = "GuiPreview-ToAddMergedDictionary";
        public static string SettingsName_PreloadedAssemblies = "GuiPreview-ToPreloadAssemblies";
        public static string SettingsName_MruAssembliesList = "GuiPreview-MostRecenltyUsedAssemblies";
    }
}
