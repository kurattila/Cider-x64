using System;
using System.Windows.Automation;
using TestStack.White.ScreenObjects;
using TestStack.White.UIItems.WindowItems;

namespace Cider_x64.GuiTests.Screens
{
    class PreviewWindow : AppScreen
    {
        public PreviewWindow(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        { }

        public virtual IntPtr GetHwnd()
        {
            return new IntPtr((int)Window.AutomationElement.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty));
        }

        public virtual Window GetWindow()
        {
            return Window;
        }
    }
}
