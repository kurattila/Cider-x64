
namespace Cider_x64
{
    internal class WindowConfiguration : Configuration
    {
        string m_WindowId;
        public WindowConfiguration(string windowId)
        {
            m_WindowId = windowId;

            Left = WindowConfiguration.UndefinedValue;
            Top = WindowConfiguration.UndefinedValue;
            Width = WindowConfiguration.UndefinedValue;
            Height = WindowConfiguration.UndefinedValue;
        }

        public override void LoadSettings()
        {
            base.LoadSettings();

            var regKeyWrapper = getAppSettingsRegistrykey();

            Left = (int)loadSingleSetting(regKeyWrapper, "Left", WindowConfiguration.UndefinedValue);
            Top = (int)loadSingleSetting(regKeyWrapper, "Top", WindowConfiguration.UndefinedValue);
            Width = (int)loadSingleSetting(regKeyWrapper, "Width", WindowConfiguration.UndefinedValue);
            Height = (int)loadSingleSetting(regKeyWrapper, "Height", WindowConfiguration.UndefinedValue);
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            var regKeyWrapper = getAppSettingsRegistrykey();

            saveSingleSetting(regKeyWrapper, "Left", Left);
            saveSingleSetting(regKeyWrapper, "Top", Top);
            saveSingleSetting(regKeyWrapper, "Width", Width);
            saveSingleSetting(regKeyWrapper, "Height", Height);
        }

        public override bool ValidSettings()
        {
            return (Left != WindowConfiguration.UndefinedValue
                && Top != WindowConfiguration.UndefinedValue
                && Width != WindowConfiguration.UndefinedValue
                && Height != WindowConfiguration.UndefinedValue);
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        internal static readonly int UndefinedValue = -10000;
        protected override RegistryKeyWrapper getAppSettingsRegistrykey()
        {
            var registryKeyWrapper = base.getAppSettingsRegistrykey();

            registryKeyWrapper.CreateSubKey(m_WindowId);
            registryKeyWrapper = registryKeyWrapper.OpenSubKey(m_WindowId, true);

            return registryKeyWrapper;
        }
    }
}
