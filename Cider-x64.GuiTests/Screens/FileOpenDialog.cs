using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestStack.White.ScreenObjects;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace Cider_x64.GuiTests.Screens
{
    class FileOpenDialog : AppScreen
    {
        public FileOpenDialog(Window window, ScreenRepository screenRepository) : base(window, screenRepository)
        { }

        public virtual string FilePath
        {
            get
            {
                var filenameEditBox = Window.Get(SearchCriteria.ByAutomationId("1148").AndControlType(typeof(TextBox), WindowsFramework.Win32)) as TextBox;
                return filenameEditBox.Text;
            }
            set
            {
                var filenameEditBox = Window.Get(SearchCriteria.ByAutomationId("1148").AndControlType(typeof(TextBox), WindowsFramework.Win32)) as TextBox;
                filenameEditBox.Text = value;
            }
        }

        public virtual void ClickOk()
        {
            var okButton = Window.Get(SearchCriteria.ByAutomationId("1").AndControlType(typeof(Button), WindowsFramework.Win32)) as Button;
            okButton.Click();
        }
    }
}
