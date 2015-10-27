using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;

namespace Cider_x64.UnitTests
{
    class Fake_Loader : Loader
    {
        class DummyType
        { }

        public bool ForceAssemblyNotFound = false;
        public List<AssemblyWrapper> LoadedAssemblies = new List<AssemblyWrapper>();
        protected override AssemblyWrapper loadAssembly(string assemblyPath)
        {
            if (ForceAssemblyNotFound)
                throw new System.IO.FileNotFoundException();

            var wrapper = new AssemblyWrapper() { Path = assemblyPath, Assembly = null };
            LoadedAssemblies.Add(wrapper);
            return wrapper;
        }

        public List<AssemblyWrapper> AssembliesRequestedToCreateFrom = new List<AssemblyWrapper>();
        public List<string> TypesRequestedToCreate = new List<string>();
        public object ForcedCreatedInstance = null;
        protected override object createInstanceOfType(AssemblyWrapper assemblyOfType, string namespaceDotType)
        {
            AssembliesRequestedToCreateFrom.Add(assemblyOfType);
            TypesRequestedToCreate.Add(namespaceDotType);
            return ForcedCreatedInstance;
        }

        public string WindowDisplayedTitleText
        {
            get
            {
                return m_GuiPreviewer.PreviewerWindow.Title;
            }
        }

        public Window WindowDisplayed;
        public override void Show()
        {
            WindowDisplayed = m_GuiPreviewer.PreviewerWindow;
        }
    }

    [TestClass]
    public class Loader_Test
    {
        public Loader_Test()
        {
        }

        [TestMethod]
        public void StaticConstructor_WillSetDesignModeForAppDomain_Always()
        {
            var loader = new Loader();

            Assert.IsTrue(System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()));
        }

        [TestMethod]
        public void AddMergedDictionary_WontThrow_WhenNoResDictSpecified()
        {
            var loader = new Fake_Loader();

            loader.AddMergedDictionary(null);

            // Implicit assert -- when at this point, then AddMergedDictionary() didn't throw
        }

        [TestMethod]
        public void AddMergedDictionary_WillAddResDictToCollection_WhenResDictSpecified()
        {
            var loader = new Fake_Loader();

            loader.AddMergedDictionary("pack://application:,,,/Cider-x64.UnitTests;component/DummyResourceDictionary.xaml");

            var resDictEnumerator = Application.Current.Resources.MergedDictionaries.GetEnumerator();
            resDictEnumerator.MoveNext();
            Assert.AreEqual(new Uri("pack://application:,,,/Cider-x64.UnitTests;component/DummyResourceDictionary.xaml"), resDictEnumerator.Current.Source);
        }

        [TestMethod]
        public void AddMergedDictionary_WillCreateApplicationObject_WhenNoApplicationObjectExistsYet()
        {
            var loader = new Fake_Loader();

            loader.AddMergedDictionary("pack://application:,,,/Cider-x64.UnitTests;component/DummyResourceDictionary.xaml");

            Assert.IsTrue(Application.Current != null);
        }

        [TestMethod]
        public void PreloadAssembly_WillLoadAssembly_Always()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.PreloadAssembly(assemblyPath);

            Assert.AreEqual(assemblyPath, loader.LoadedAssemblies[0].Path);
        }

        [TestMethod]
        public void Load_WillLoadAssembly_Always()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new UserControl();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.Load(assemblyPath, "Namespace.Type");

            Assert.AreEqual(assemblyPath, loader.LoadedAssemblies[0].Path);
        }

        [TestMethod]
        public void Load_WillCreateInstanceOfType_Always()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new UserControl();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.Load(assemblyPath, "dummyNamespace.dummyType");

            Assert.AreEqual(assemblyPath, loader.AssembliesRequestedToCreateFrom[0].Path);
            Assert.IsTrue(loader.TypesRequestedToCreate.Contains("dummyNamespace.dummyType"));
        }

        [TestMethod]
        public void Load_WillHandleFileException_WhenAssemblyNotFound()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.ForceAssemblyNotFound = true;
            loader.Load(assemblyPath, "dummyNamespace.dummyType");

            // Implicit assert: exception thrown by Show() would make this test fail
        }

        [TestMethod]
        public void Load_WillQuitImmediately_WhenAssemblyPathEmpty()
        {
            var loader = new Fake_Loader();

            loader.Load("", "dummyNamespace.dummyType");

            Assert.AreEqual(0, loader.LoadedAssemblies.Count);
        }

        [TestMethod]
        public void Load_WillQuitImmediately_WhenTypeToCreateIsEmpty()
        {
            var loader = new Fake_Loader();

            loader.Load("dummyAssembly.dll", "");

            Assert.AreEqual(0, loader.LoadedAssemblies.Count);
        }

        [TestMethod]
        public void Show_WillDisplayWindow_WhenWindowTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.Load("dummyAssembly.dll", "dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual(dummyWindow, loader.WindowDisplayed);
        }

        [TestMethod]
        public void Show_WillDisplayUserControl_WhenUserControlTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;
            loader.Load("dummyAssembly.dll", "dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual(dummyUserControl, loader.WindowDisplayed.Content);
        }

        [TestMethod]
        public void Show_WillSetHostingWindowTitleToTypeName_WhenPreviewingUserControl()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;
            loader.Load("dummyAssembly.dll", "dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual("dummyNamespace.dummyType", loader.WindowDisplayed.Title);
        }

        [TestMethod]
        public void Show_WillSetWindowTitleToTypeName_WhenPreviewingWindow()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.Load("dummyAssembly.dll", "dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual("dummyNamespace.dummyType", loader.WindowDisplayedTitleText);
        }

        [TestMethod]
        public void Load_WillCreatePreviewWindow_Always()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.Load("dummyAssembly.dll", "dummyNamespace.dummyType");
        }
    }
}
