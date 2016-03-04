﻿
using System;

namespace Cider_x64
{
    public class WindowConfiguration : Configuration
    {
        string m_WindowId;
        public WindowConfiguration(string windowId)
        {
            m_WindowId = windowId;

            Left = WindowConfiguration.UndefinedValue;
            Top = WindowConfiguration.UndefinedValue;
            Width = WindowConfiguration.UndefinedValue;
            Height = WindowConfiguration.UndefinedValue;
            IsTopMostWindow = true;
        }

        public override void LoadSettings()
        {
            base.LoadSettings();

            var regKeyWrapper = getAppSettingsRegistrykey();

            Left = (int)loadSingleSetting(regKeyWrapper, "Left", WindowConfiguration.UndefinedValue);
            Top = (int)loadSingleSetting(regKeyWrapper, "Top", WindowConfiguration.UndefinedValue);
            Width = (int)loadSingleSetting(regKeyWrapper, "Width", WindowConfiguration.UndefinedValue);
            Height = (int)loadSingleSetting(regKeyWrapper, "Height", WindowConfiguration.UndefinedValue);

            int isTopMostWindowRaw = (int)loadSingleSetting(regKeyWrapper, "IsTopMostWindow", 0);
            IsTopMostWindow = isTopMostWindowRaw != 0;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            var regKeyWrapper = getAppSettingsRegistrykey();

            saveSingleSetting(regKeyWrapper, "Left", Left);
            saveSingleSetting(regKeyWrapper, "Top", Top);
            saveSingleSetting(regKeyWrapper, "Width", Width);
            saveSingleSetting(regKeyWrapper, "Height", Height);
            saveSingleSetting(regKeyWrapper, "IsTopMostWindow", IsTopMostWindow ? 1 : 0);
        }

        public override bool ValidSettings()
        {
            bool valid = Left != WindowConfiguration.UndefinedValue
                      && Top != WindowConfiguration.UndefinedValue
                      && Width != WindowConfiguration.UndefinedValue
                      && Height != WindowConfiguration.UndefinedValue;
            if (Width < 1 || Height < 1)
                valid = false;
            return valid;
        }

        public virtual int Left { get; set; }
        public virtual int Top { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual bool IsTopMostWindow { get; set; }

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
