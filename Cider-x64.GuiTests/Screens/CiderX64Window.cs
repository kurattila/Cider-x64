using TestStack.White;
using TestStack.White.Factory;
using TestStack.White.ScreenObjects;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.WindowItems;

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
    }
}
