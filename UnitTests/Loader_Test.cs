using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;

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

        public void SetAlternativeGuiTypesExtractor(GuiTypesExtractor alternativeExtractor)
        {
            TypesExtractor = alternativeExtractor;
        }

        public IGuiPreviewer GetGuiPreviewer()
        {
            return m_GuiPreviewer;
        }
    }

    class Fake2_Loader : Loader
    {
        public bool ForceAssemblyNotFound = false;
        protected override AssemblyWrapper loadAssembly(string assemblyPath)
        {
            if (ForceAssemblyNotFound)
                throw new System.IO.FileNotFoundException();

            var wrapper = new AssemblyWrapper() { Path = assemblyPath, Assembly = null };
            return wrapper;
        }

        public void SetAlternativeGuiTypesExtractor(GuiTypesExtractor alternativeExtractor)
        {
            TypesExtractor = alternativeExtractor;
        }
    }

    class Fake3_Loader : Loader
    {
        public Exception ForcedThrowExceptionDuringCreateInstance = null;
        protected override object createInstanceOfType(AssemblyWrapper assemblyOfType, string namespaceDotType)
        {
            throw ForcedThrowExceptionDuringCreateInstance;
        }

        public void SetAlternativeGuiTypesExtractor(GuiTypesExtractor alternativeExtractor)
        {
            TypesExtractor = alternativeExtractor;
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
        public void LoadAssembly_WillLoadAssembly_Always()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new UserControl();
            string assemblyPath = @"\somePath\dummyAssembly.dll";
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());

            loader.LoadAssembly(assemblyPath);

            Assert.AreEqual(assemblyPath, loader.LoadedAssemblies[0].Path);
        }

        [TestMethod]
        public void LoadType_WillCreateInstanceOfType_Always()
        {
            var loader = new Fake_Loader();
            loader.ForcedCreatedInstance = new UserControl();
            string assemblyPath = @"\somePath\dummyAssembly.dll";
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly(assemblyPath);
            
            loader.LoadType("dummyNamespace.dummyType");

            Assert.AreEqual(assemblyPath, loader.AssembliesRequestedToCreateFrom[0].Path);
            Assert.IsTrue(loader.TypesRequestedToCreate.Contains("dummyNamespace.dummyType"));
        }

        [TestMethod]
        public void LoadAssembly_WillHandleFileException_WhenAssemblyNotFound()
        {
            var loader = new Fake_Loader();
            string assemblyPath = @"\somePath\dummyAssembly.dll";

            loader.ForceAssemblyNotFound = true;
            loader.LoadAssembly(assemblyPath);

            // Implicit assert: exception thrown by Show() would make this test fail
        }

        [TestMethod]
        public void LoadAssembly_WillQuitImmediately_WhenAssemblyPathEmpty()
        {
            var loader = new Fake_Loader();

            loader.LoadAssembly("");

            Assert.AreEqual(0, loader.LoadedAssemblies.Count);
        }

        [TestMethod]
        public void LoadType_WillQuitImmediately_WhenTypeToCreateIsEmpty()
        {
            var loader = new Fake_Loader();
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            
            loader.LoadType("");

            Assert.AreEqual(0, loader.TypesRequestedToCreate.Count);
        }

        [TestMethod]
        public void Show_WillDisplayWindow_WhenWindowTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            loader.LoadType("dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual(dummyWindow, loader.WindowDisplayed);
        }

        [TestMethod]
        public void Show_WillDisplayUserControl_WhenUserControlTypeInstantiated()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            loader.LoadType("dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual(dummyUserControl, loader.WindowDisplayed.Content);
        }

        [TestMethod]
        public void Show_WillSetHostingWindowTitleToTypeName_WhenPreviewingUserControl()
        {
            var loader = new Fake_Loader();
            var dummyUserControl = new UserControl();
            loader.ForcedCreatedInstance = dummyUserControl;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            loader.LoadType("dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual("dummyNamespace.dummyType", loader.WindowDisplayed.Title);
        }

        [TestMethod]
        public void Show_WillSetWindowTitleToTypeName_WhenPreviewingWindow()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            loader.LoadType("dummyNamespace.dummyType");

            loader.Show();

            Assert.AreEqual("dummyNamespace.dummyType", loader.WindowDisplayedTitleText);
        }

        [TestMethod]
        public void Show_WontThrow_WhenNamespaceDotTypeEmpty()
        {
            var loader = new Fake2_Loader();
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            var dummyWindow = new Window();
            loader.LoadAssembly("dummyAssembly.dll");

            loader.LoadType("");

            loader.Show();
        }

        [TestMethod]
        public void LoadType_WillCreateGuiPreviewer_WhenNamespaceDotTypeNotEmpty()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");

            loader.LoadType("dummyNamespace.dummyType");

            Assert.IsNotNull(loader.GetGuiPreviewer());
        }

        [TestMethod]
        public void LoadType_WontCreateGuiPreviewer_WhenNamespaceDotTypeEmpty()
        {
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");

            loader.LoadType("");

            Assert.IsNull(loader.GetGuiPreviewer());
        }

        [TestMethod]
        public void LoadType_WillThrowMissingPreloadException_WhenCreateInstanceThrowsFileNotFound()
        {
            var loader = new Fake3_Loader();
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            loader.LoadAssembly("dummyAssembly.dll");
            var fileNotFoundException = new FileNotFoundException("", "dummyAssemblyStyles.dll");
            var xamlParseException = new XamlParseException("", fileNotFoundException);
            var targetInvocationException = new TargetInvocationException(xamlParseException);
            loader.ForcedThrowExceptionDuringCreateInstance = targetInvocationException;
            Exception thrownMissingPreloadException = null;

            try
            {
                loader.LoadType("dummyNamespace.dummyType");
            }
            catch(MissingPreloadException e)
            {
                thrownMissingPreloadException = e;
            }

            Assert.IsNotNull(thrownMissingPreloadException);
            Assert.IsInstanceOfType(thrownMissingPreloadException.InnerException, typeof(TargetInvocationException));
            Assert.IsInstanceOfType(thrownMissingPreloadException.InnerException.InnerException, typeof(XamlParseException));
            Assert.IsNotNull(thrownMissingPreloadException.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void GetLoadedAssemblyTypeNames_ReturnsTypesFromGuiTypesExtractor_Always()
        {
            var stubGuiTypesExtractor = new Fake_GuiTypesExtractor();
            stubGuiTypesExtractor.ForcedExtractedGuiTypes.Add(typeof(UserControl));
            stubGuiTypesExtractor.ForcedExtractedGuiTypes.Add(typeof(Window));
            var loader = new Fake_Loader();
            var dummyWindow = new Window();
            loader.ForcedCreatedInstance = dummyWindow;
            loader.SetAlternativeGuiTypesExtractor(stubGuiTypesExtractor);
            loader.LoadAssembly("dummyAssembly.dll");

            var guiTypeNames = loader.GetLoadedAssemblyTypeNames();

            Assert.AreEqual(2, guiTypeNames.Count);
        }

        [TestMethod]
        public void CloseWindow_WontThrow_WhenNamespaceDotTypeEmpty()
        {
            var loader = new Fake2_Loader();
            loader.SetAlternativeGuiTypesExtractor(new Fake_GuiTypesExtractor());
            var dummyWindow = new Window();
            loader.LoadAssembly("dummyAssembly.dll");
            loader.LoadType("");

            loader.Show();

            loader.CloseWindow();
        }

        class Fake_GuiTypesExtractor : GuiTypesExtractor
        {
            public List<Type> ForcedExtractedGuiTypes = new List<Type>();
            public override List<Type> GetGuiTypesOnly(AssemblyWrapper assemblyWrapper)
            {
                return ForcedExtractedGuiTypes;
            }
        }
    }
}
