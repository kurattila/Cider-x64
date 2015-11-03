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
            // Workaround needed to be able to use "pack://application..." syntax.
            // http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            // The protocol "pack://" won't be recognized before it is registered and its registration can be forced
            // by force-creating an Application object (if none exists yet)
            if (Application.Current == null)
                new Application();
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// 
        /// Prevent exceptions of "Object 'XXX.rem' has been disconnected or does not exist at the server" happening
        /// because of an expired lease of a remoting object.
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            // From "Remoting: Managing the Lifetime of Remote .NET Objects with Leasing and Sponsorship"
            // in MSDN Magazine December 2003:
            // 
            // The singleton design pattern semantics mandate that the singleton object lives forever
            // once it is created. This can't happen if its default lease time is five minutes.
            // If no client accesses a singleton object for more than five minutes after it is created,
            // the .NET Remoting Framework will deactivate the singleton. Future calls from the clients
            // will be silently routed to a new singleton object in violation of the pattern's semantics.
            // Fortunately, infinite lease time is supported. When you design a singleton object,
            // override InitializeLifetimeService and return a null object as the new lease,
            // indicating that this lease never expires
            return null;
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

            m_LoadedAssemblyTypes = getValidAssemblyTypeNames(wrapper);

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


        private System.Collections.Generic.List<string> m_LoadedAssemblyTypes;

        virtual public System.Collections.Generic.List<string> GetLoadedAssemblyTypeNames()
        {
            return m_LoadedAssemblyTypes;
        }

        virtual protected System.Collections.Generic.List<string> getValidAssemblyTypeNames(AssemblyWrapper assemblyWrapper)
        {
            Type[] assemblyTypes = assemblyWrapper.Assembly.GetTypes();
            var list = new System.Collections.Generic.List<string>();

            foreach (Type type in assemblyTypes)
            {
                if (!String.IsNullOrEmpty(type.FullName) && type.IsClass)
                    list.Add(type.FullName);
            }

            return list;
        }
    }

    public struct AssemblyWrapper
    {
        public string Path;
        public Assembly Assembly;
    }
}
