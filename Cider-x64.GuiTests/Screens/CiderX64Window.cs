using System;
using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.ScreenObjects;
using TestStack.White.ScreenObjects.ScreenAttributes;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.UIItems.WindowStripControls;
using System.Windows.Automation;

namespace Cider_x64.GuiTests.Screens
{
    class CiderX64Window : AppScreen
    {
        protected Menu fileMenuItem;
        protected Menu fileOpenMenuItem;

        public CiderX64Window(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        { }

        public virtual Menus FileMenuChildItems
        {
            get { return fileMenuItem.ChildMenus; }
        }

        public virtual FileOpenDialog ShowFileOpenDialog()
        {
            fileMenuItem.Click();
            fileOpenMenuItem.Click();
            return ScreenRepository.GetModal<FileOpenDialog>("Open", Window, InitializeOption.NoCache);
        }

        public virtual CiderX64Window SetViewMenuTopMostFlag(bool topmost)
        {
            var menuItemTopMost = Window.MenuBar.MenuItem("View", "Always on Top");
            object togglePatternRaw = null;
            if (menuItemTopMost.AutomationElement.TryGetCurrentPattern(TogglePattern.Pattern, out togglePatternRaw))
            {
                var togglePattern = togglePatternRaw as TogglePattern;
                var state = togglePattern.Current.ToggleState;
                var desiredState = topmost ? ToggleState.On : ToggleState.Off;
                if (state != desiredState)
                    togglePattern.Toggle();
                Window.MenuBar.MenuItem("View").Click();
            }
            return this;
        }

        public virtual bool IsTopMostWindow()
        {
            object isTopMostRaw = Window.AutomationElement.GetCurrentPropertyValue(WindowPatternIdentifiers.IsTopmostProperty);
            return (bool)isTopMostRaw;
        }
    }
}
