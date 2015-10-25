using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class GuiPreviewerFactory_Test
    {
        [TestMethod]
        public void Factory_WillCreatePreviewerForWindow_WhenWindowSpecified()
        {
            var guiObject = new Window();

            IGuiPreviewer previewer = GuiPreviewerFactory.Create(guiObject);

            Assert.IsTrue(previewer is GuiPreviewerForWindow);
            Assert.AreEqual(guiObject, previewer.PreviewerWindow);
        }

        [TestMethod]
        public void Factory_WillCreatePreviewerForUserControl_WhenUserControlSpecified()
        {
            var guiObject = new UserControl();

            IGuiPreviewer previewer = GuiPreviewerFactory.Create(guiObject);

            Assert.IsTrue(previewer is GuiPreviewerForUserControl);
            Assert.AreEqual(guiObject, previewer.PreviewerWindow.Content);
        }
    }

    [TestClass]
    public class GuiPreviewerForWindow_Test
    {
        [TestMethod]
        public void PreviewerWindow_WontShownInTaskbar_Always()
        {
            IGuiPreviewer previewer = new GuiPreviewerForWindow(new Window());

            bool shownInTaskbar = previewer.PreviewerWindow.ShowInTaskbar;

            Assert.IsFalse(shownInTaskbar);
        }
    }

    [TestClass]
    public class GuiPreviewerForUserControl_Test
    {
        [TestMethod]
        public void PreviewerWindow_WontShownInTaskbar_Always()
        {
            IGuiPreviewer previewer = new GuiPreviewerForUserControl(new UserControl());

            bool shownInTaskbar = previewer.PreviewerWindow.ShowInTaskbar;

            Assert.IsFalse(shownInTaskbar);
        }
    }
}
