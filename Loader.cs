using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    internal class Loader : MarshalByRefObject, ILoader
    {
        WindowConfiguration m_WindowConfig = new WindowConfiguration("PreviewWindow");

        static Loader()
        {
            DesignerProperties.IsInDesignModeProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | FrameworkPropertyMetadataOptions.Inherits));
        }

        public Loader()
        {
        }

        private void onSourceInitialized(object sender, EventArgs e)
        {
            m_WindowConfig.LoadSettings();
            if (m_WindowConfig.ValidSettings())
            {
                Window previewWindow = sender as Window;
                previewWindow.Topmost = true;

                previewWindow.Left = m_WindowConfig.Left;
                previewWindow.Top = m_WindowConfig.Top;
                previewWindow.Width = m_WindowConfig.Width;
                previewWindow.Height = m_WindowConfig.Height;
            }
        }

        private void onPreviewWindowClosed(object sender, EventArgs e)
        {
            Window previewWindow = sender as Window;

            m_WindowConfig.Left = (int)previewWindow.Left;
            m_WindowConfig.Top = (int)previewWindow.Top;
            m_WindowConfig.Width = (int)previewWindow.Width;
            m_WindowConfig.Height = (int)previewWindow.Height;
            m_WindowConfig.SaveSettings();
        }

        public void AddMergedDictionary(string packUriStringOfResDictXaml)
        {
            if (string.IsNullOrEmpty(packUriStringOfResDictXaml))
                return;

            ResourceDictionary xamlDictionaryToMerge = new ResourceDictionary();
            xamlDictionaryToMerge.Source = new Uri(packUriStringOfResDictXaml);

            var mergedDictionaries = getResourceDictionariesCollection();
            mergedDictionaries.Add(xamlDictionaryToMerge);
        }

        virtual protected Collection<ResourceDictionary> getResourceDictionariesCollection()
        {
            if (Application.Current == null)
                new Application();
            return Application.Current.Resources.MergedDictionaries;
        }

        virtual protected AssemblyWrapper loadAssembly(string assemblyPath)
        {
            var wrapper = new AssemblyWrapper() { Path = assemblyPath };
            wrapper.Assembly = Assembly.LoadFrom(assemblyPath);
            return wrapper;
        }

        public void PreloadAssembly(string assemblyPath)
        {
            loadAssembly(assemblyPath);
        }

        protected IGuiPreviewer m_GuiPreviewer;
        object m_InstanceCreated;
        string m_NamespaceDotTypeCreated;
        public virtual void Load(string assemblyPath, string namespaceDotType)
        {
            if (string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(namespaceDotType))
                return; // settings uninitialized

            AssemblyWrapper wrapper;
            try
            {
                wrapper = loadAssembly(assemblyPath);
            }
            catch(FileNotFoundException)
            {
                return; // wrong assembly path specified
            }

            m_InstanceCreated = createInstanceOfType(wrapper, namespaceDotType);
            m_NamespaceDotTypeCreated = namespaceDotType;

            m_GuiPreviewer = GuiPreviewerFactory.Create(m_InstanceCreated);
            m_GuiPreviewer.PreviewerWindow.Title = m_NamespaceDotTypeCreated;
            m_GuiPreviewer.PreviewerWindow.SourceInitialized += onSourceInitialized;
            m_GuiPreviewer.PreviewerWindow.Closed += onPreviewWindowClosed;
        }

        public virtual void Hide()
        {
            m_GuiPreviewer.PreviewerWindow.Hide();
        }

        public virtual void Show()
        {
            m_GuiPreviewer.PreviewerWindow.Show();
        }

        public void CloseWindow()
        {
            if (m_GuiPreviewer.PreviewerWindow.IsVisible)
                m_GuiPreviewer.PreviewerWindow.Close();
        }

        virtual protected object createInstanceOfType(AssemblyWrapper assemblyOfType, string namespaceDotType)
        {
            Type typeToCreate = assemblyOfType.Assembly.GetType(namespaceDotType);
            return Activator.CreateInstance(typeToCreate);
        }
    }

    public struct AssemblyWrapper
    {
        public string Path;
        public Assembly Assembly;
    }
}
