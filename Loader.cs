using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    public class Loader : MarshalByRefObject, ILoader
    {
        Window m_Win = new Window() { Topmost = true };
        WindowConfiguration m_WindowConfig = new WindowConfiguration("PreviewWindow");

        static Loader()
        {
            DesignerProperties.IsInDesignModeProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | FrameworkPropertyMetadataOptions.Inherits));
        }

        public Loader()
        {
            m_Win.Initialized += onPreviewWindowInitialized;
            m_Win.Closed += onPreviewWindowClosed;
        }

        private void onPreviewWindowInitialized(object sender, EventArgs e)
        {
            m_WindowConfig.LoadSettings();
            if (m_WindowConfig.ValidSettings())
            {
                m_Win.Left = m_WindowConfig.Left;
                m_Win.Top = m_WindowConfig.Top;
                m_Win.Width = m_WindowConfig.Width;
                m_Win.Height = m_WindowConfig.Height;
            }
        }

        private void onPreviewWindowClosed(object sender, EventArgs e)
        {
            m_WindowConfig.Left = (int)m_Win.Left;
            m_WindowConfig.Top = (int)m_Win.Top;
            m_WindowConfig.Width = (int)m_Win.Width;
            m_WindowConfig.Height = (int)m_Win.Height;
            m_WindowConfig.SaveSettings();
        }

        public void AddMergedDictionary(string packUriStringOfResDictXaml)
        {
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

        public void Show(string assemblyPath, string namespaceDotType)
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

            object instanceCreated = createInstanceOfType(wrapper, namespaceDotType);

            if (instanceCreated is Window)
                displayWpfGuiPreview(instanceCreated as Window);

            if (instanceCreated is UserControl)
                displayWpfGuiPreview(instanceCreated as UserControl);
        }

        public void CloseWindow()
        {
            if (m_Win.IsVisible)
                m_Win.Close();
        }

        virtual protected object createInstanceOfType(AssemblyWrapper assemblyOfType, string namespaceDotType)
        {
            Type typeToCreate = assemblyOfType.Assembly.GetType(namespaceDotType);
            return Activator.CreateInstance(typeToCreate);
        }

        virtual protected void displayWpfGuiPreview(Window instanceCreated)
        {
            instanceCreated.Show();
        }

        virtual protected void displayWpfGuiPreview(UserControl instanceCreated)
        {
            m_Win.Content = instanceCreated;
            m_Win.Show();
        }
    }

    public struct AssemblyWrapper
    {
        public string Path;
        public Assembly Assembly;
    }
}
