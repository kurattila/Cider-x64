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

        public Window WindowDisplayed;
        public string WindowDisplayedTitleText;
        protected override void displayWpfGuiPreview(Window instanceCreated, string windowTitle)
        {
            WindowDisplayed = instanceCreated;
            WindowDisplayedTitleText = windowTitle;
        }

        public UserControl UserControlDisplayed;
        public string UserControlHostingWindowTitleText;
        protected override void displayWpfGuiPreview(UserControl instanceCreated, string windowTitle)
        {
            UserControlDisplayed = instanceCreated;
            UserControlHostingWindowTitleText = windowTitle;
        }
    }

    [TestClass]
    public class Loader_Test
    {
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
        public void Show_WillLoadAssemblyBeforeShowing_Always()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.Show(assemblyPath, "Namespace.Type");

            Assert.AreEqual(assemblyPath, loader.LoadedAssemblies[0].Path);
        }

        [TestMethod]
        public void Show_WillCreateInstanceOfType_Always()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.Show(assemblyPath, "dummyNamespace.dummyType");

            Assert.AreEqual(assemblyPath, loader.AssembliesRequestedToCreateFrom[0].Path);
            Assert.IsTrue(loader.TypesRequestedToCreate.Contains("dummyNamespace.dummyType"));
        }

        [TestMethod]
        public void Show_WillHandleFileException_WhenAssemblyNotFound()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.ForceAssemblyNotFound = true;
            loader.Show(assemblyPath, "dummyNamespace.dummyType");

            // Implicit assert: exception thrown by Show() would make this test fail
        }

        [TestMethod]
        public void Show_WillQuitImmediately_WhenAssemblyPathEmpty()
        {
            var loader = new Fake_Loader();

            loader.Show("", "dummyNamespace.dummyType");

            Assert.AreEqual(0, loader.LoadedAssemblies.Count);
        }

        [TestMethod]
        public void Show_WillQuitImmediately_WhenTypeToCreateIsEmpty()
        {
            var loader = new Fake_Loader();

            loader.Show("dummyAssembly.dll", "");

            Assert.AreEqual(0, loader.LoadedAssemblies.Count);
        }

        [TestMethod]
        public void Show_WillDisplayWindow_WhenWindowTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.AreEqual(dummyWindow, loader.WindowDisplayed);
            Assert.IsTrue(loader.UserControlDisplayed == null);
        }

        [TestMethod]
        public void Show_WontDisplayWindow_WhenNonWindowTypeInstantiated()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new object();

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.IsTrue(loader.UserControlDisplayed == null);
            Assert.IsTrue(loader.WindowDisplayed == null);
        }

        [TestMethod]
        public void Show_WillDisplayUserControl_WhenUserControlTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.AreEqual(dummyUserControl, loader.UserControlDisplayed);
            Assert.IsTrue(loader.WindowDisplayed == null);
        }

        [TestMethod]
        public void Show_WontDisplayUserControl_WhenNonUserControlTypeInstantiated()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new object();

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.IsTrue(loader.UserControlDisplayed == null);
            Assert.IsTrue(loader.WindowDisplayed == null);
        }

        [TestMethod]
        public void Show_WillSetHostingWindowTitleToTypeName_WhenPreviewingUserControl()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.AreEqual("dummyNamespace.dummyType", loader.UserControlHostingWindowTitleText);
        }

        [TestMethod]
        public void Show_WillSetWindowTitleToTypeName_WhenPreviewingWindow()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;

            loader.Show("dummyAssembly.dll", "dummyNamespace.dummyType");

            Assert.AreEqual("dummyNamespace.dummyType", loader.WindowDisplayedTitleText);
        }
    }
}
