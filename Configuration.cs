using Microsoft.Win32;

namespace Cider_x64
{
    public class Configuration
    {
        public virtual void LoadSettings()
        {
        }

        public virtual void SaveSettings()
        {
        }

        public virtual bool ValidSettings()
        {
            return false;
        }



        protected virtual RegistryKeyWrapper getHKCU()
        {
            return new RegistryKeyWrapper() { RegistryKey = Registry.CurrentUser, RegPath = "HKEY_CURRENT_USER" };
        }

        protected virtual void saveSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId, object value)
        {
            regKeyWrapper.RegistryKey.SetValue(settingId, value);
        }

        protected virtual object loadSingleSetting(RegistryKeyWrapper regKeyWrapper, string settingId, object defaultValue)
        {
            return regKeyWrapper.RegistryKey.GetValue(settingId, defaultValue);
        }

        string m_AppRegistryBranch = "Cider-x64";
        string m_AppVersion = "1.0.0";

        private RegistryKeyWrapper m_RegKeyWrapper;
        protected virtual RegistryKeyWrapper getAppSettingsRegistrykey()
        {
            if (m_RegKeyWrapper == null)
            {
                m_RegKeyWrapper = new RegistryKeyWrapper();

                RegistryKeyWrapper hkcu = getHKCU();
                RegistryKeyWrapper key = hkcu.OpenSubKey("Software", true);
                key.CreateSubKey(m_AppRegistryBranch);
                key = key.OpenSubKey(m_AppRegistryBranch, true);

                key.CreateSubKey(m_AppVersion);
                key = key.OpenSubKey(m_AppVersion, true);

                m_RegKeyWrapper = key;
            }

            return m_RegKeyWrapper;
        }
    }

    public class RegistryKeyWrapper
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
