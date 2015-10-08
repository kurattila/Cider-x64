using System;
using System.Collections.Generic;
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
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            var regKey = getAppSettingsRegistrykey();
            saveSingleSetting(regKey, SettingsName_AssemblyFullPath, AssemblyOfPreviewedGui);
            saveSingleSetting(regKey, SettingsName_Type, TypeOfPreviewedGui);
            saveSingleSetting(regKey, SettingsName_ToAddMergedDictionary, ResourceDictionaryToAdd);
            saveSingleSetting(regKey, SettingsName_PreloadedAssemblies, PreloadedAssemblies.ToArray());
        }

        public override bool ValidSettings()
        {
            return string.IsNullOrEmpty(AssemblyOfPreviewedGui) == false;
        }

        public string AssemblyOfPreviewedGui { get; set; }

        public string TypeOfPreviewedGui { get; set; }

        public string ResourceDictionaryToAdd { get; set; }

        public List<string> PreloadedAssemblies { get; set; }

        public static string SettingsName_AssemblyFullPath = "GuiPreview-AssemblyFullPath";
        public static string SettingsName_Type = "GuiPreview-Namespace.TypeName";
        public static string SettingsName_ToAddMergedDictionary = "GuiPreview-ToAddMergedDictionary";
        public static string SettingsName_PreloadedAssemblies = "GuiPreview-ToPreloadAssemblies";
    }
}
