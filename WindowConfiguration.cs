using Microsoft.Win32;
using System.Windows;

namespace Cider_x64
{
    internal class WindowConfiguration
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

        public void LoadSettings()
        {
            var regKeyWrapper = getAppSettingsRegistrykey();

            Left = loadSingleSetting(regKeyWrapper, "Left");
            Top = loadSingleSetting(regKeyWrapper, "Top");
            Width = loadSingleSetting(regKeyWrapper, "Width");
            Height = loadSingleSetting(regKeyWrapper, "Height");
        }

        public void SaveSettings()
        {
            var regKeyWrapper = getAppSettingsRegistrykey();

            saveSingleSetting(regKeyWrapper, "Left", Left);
            saveSingleSetting(regKeyWrapper, "Top", Top);
            saveSingleSetting(regKeyWrapper, "Width", Width);
            saveSingleSetting(regKeyWrapper, "Height", Height);
        }

        public bool ValidSettings()
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

        string m_AppRegistryBranch = "Cider-x64";
        string m_AppVersion = "1.0.0";
        static readonly int UndefinedValue = -10000;

        RegistryKeyWrapper m_RegKeyWrapper;
        RegistryKeyWrapper getAppSettingsRegistrykey()
        {
            if (m_RegKeyWrapper == null)
            {
                m_RegKeyWrapper = new RegistryKeyWrapper();

                //                 RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                RegistryKeyWrapper hkcu = getHKCU();
                RegistryKeyWrapper key = hkcu.OpenSubKey("Software", true);
                key.CreateSubKey(m_AppRegistryBranch);
                key = key.OpenSubKey(m_AppRegistryBranch, true);

                key.CreateSubKey(m_AppVersion);
                key = key.OpenSubKey(m_AppVersion, true);

                key.CreateSubKey(m_WindowId);
                key = key.OpenSubKey(m_WindowId, true);

                m_RegKeyWrapper = key;
            }

            return m_RegKeyWrapper;
        }

        protected virtual RegistryKeyWrapper getHKCU()
        {
            return new RegistryKeyWrapper() { RegistryKey = Registry.CurrentUser, RegPath = "HKEY_CURRENT_USER" };
        }

        protected virtual void saveSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId, int value)
        {
            regKeyWrapper.RegistryKey.SetValue(settingId, value);
        }

        protected virtual int loadSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId)
        {
            int value = WindowConfiguration.UndefinedValue;
            object rawValue = regKeyWrapper.RegistryKey.GetValue(settingId);
            if (rawValue != null)
                value = (int)rawValue;
            return value;
        }
    }

    internal class RegistryKeyWrapper
    {
        public string RegPath;
        public RegistryKey RegistryKey;

        public RegistryKeyWrapper OpenSubKey(string name, bool writable)
        {
            var openedSubKey = new RegistryKeyWrapper();
            if (RegistryKey != null)
                openedSubKey.RegistryKey = this.RegistryKey.OpenSubKey(name, writable);

            openedSubKey.RegPath = string.Format(@"{0}\{1}", this.RegPath, name);
            return openedSubKey;
        }

        public RegistryKeyWrapper CreateSubKey(string subkey)
        {
            var createdSubKey = new RegistryKeyWrapper();
            if (RegistryKey != null)
                createdSubKey.RegistryKey = this.RegistryKey.CreateSubKey(subkey);

            createdSubKey.RegPath = this.RegPath;
            return createdSubKey;
        }
    }
}
