using System;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    internal interface IGuiPreviewer
    {
        Window PreviewerWindow { get; }
    }

    internal static class GuiPreviewerFactory
    {
        public static IGuiPreviewer Create(object guiInstanceToCreatePreviewerFor)
        {
            if (guiInstanceToCreatePreviewerFor is Window)
                return new GuiPreviewerForWindow(guiInstanceToCreatePreviewerFor as Window);
            else if (guiInstanceToCreatePreviewerFor is UserControl)
                return new GuiPreviewerForUserControl(guiInstanceToCreatePreviewerFor as UserControl);
            return null;
        }
    }

    internal class GuiPreviewerForWindow : IGuiPreviewer
    {
        Window m_PreviewerWindow;
        public GuiPreviewerForWindow(Window guiInstanceToCreatePreviewerFor)
        {
            m_PreviewerWindow = guiInstanceToCreatePreviewerFor;
            m_PreviewerWindow.ShowInTaskbar = false;
            m_PreviewerWindow.Topmost = true;
        }

        public Window PreviewerWindow
        {
            get
            {
                return m_PreviewerWindow;
            }
        }
    }

    internal class GuiPreviewerForUserControl : IGuiPreviewer
    {
        Window m_PreviewerWindow;

        public GuiPreviewerForUserControl(UserControl guiInstanceToCreatePreviewerFor)
        {
            m_PreviewerWindow = new Window()
            {
                Content = guiInstanceToCreatePreviewerFor,
                Topmost = true,
                ShowInTaskbar = false
            };
        }

        public Window PreviewerWindow
        {
            get
            {
                return m_PreviewerWindow;
            }
        }
    }
}
