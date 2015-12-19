using System;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class SwitcherOfLoadedType_Test
    {
        class Fake_SwitcherOfLoadedType : SwitcherOfLoadedType
        {
            protected override void showMessageBoxSeam(string message, string caption)
            { }
        }

        [TestMethod]
        public void ToggleType_WillClosePreview_WhenSwitchingOnAnotherType()
        {
            var loaderConfig = new LoaderConfiguration();
            var spyLoader = new Mock<ILoader>();
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = new MainViewModel(), LoaderConfiguration = loaderConfig };

            switcher.ToggleType("dummyNamespace.dummyGuiType");

            spyLoader.Verify(loader => loader.CloseWindow(), Times.Exactly(1));
        }

        [TestMethod]
        public void ToggleType_WillClosePreview_WhenSwitchingOffCurrentType()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loaderConfig = new LoaderConfiguration();
            var spyLoader = new Mock<ILoader>();
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = mainVM, LoaderConfiguration = loaderConfig };

            switcher.ToggleType("b.b");

            spyLoader.Verify(loader => loader.CloseWindow(), Times.Exactly(1));
        }

        [TestMethod]
        public void ToggleType_WillLoadTypeX_WhenTypeXNotShownCurrently()
        {
            var loaderConfig = new LoaderConfiguration();
            var spyLoader = new Mock<ILoader>();
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = new MainViewModel(), LoaderConfiguration = loaderConfig };

            switcher.ToggleType("dummyNamespace.dummyGuiType");

            spyLoader.Verify(loader => loader.LoadType("dummyNamespace.dummyGuiType"), Times.Exactly(1));
        }

        [TestMethod]
        public void ToggleType_WillMarkTypeXAsShown_WhenTypeXNotShownCurrently()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b" });
            var loaderConfig = new LoaderConfiguration();
            var loader = new Mock<ILoader>();
            var switcher = new SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = loaderConfig };

            switcher.ToggleType("b.b");

            Assert.AreEqual(true, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
        }

        [TestMethod]
        public void ToggleType_WillMarkAllTypesUnshown_WhenTypeAlreadyShown()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loaderConfig = new LoaderConfiguration();
            var spyLoader = new Mock<ILoader>();
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = mainVM, LoaderConfiguration = loaderConfig };

            switcher.ToggleType("b.b");

            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[0].IsShown);
            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
            spyLoader.Verify(loader => loader.LoadType("b.b"), Times.Never());
        }

        [TestMethod]
        public void ToggleType_WillMarkAllTypesUnshown_WhenTypeAlreadyShownAndCloseWindowFiresPreviewWindowClosed()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loaderConfig = new LoaderConfiguration();
            var spyLoader = new Mock<ILoader>();
            spyLoader.Setup(l => l.CloseWindow()).Raises(loader => loader.PreviewWindowClosed += null, EventArgs.Empty);
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = mainVM, LoaderConfiguration = loaderConfig };

            switcher.ToggleType("b.b");

            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[0].IsShown);
            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
            spyLoader.Verify(loader => loader.LoadType("b.b"), Times.Never());
        }

        [TestMethod]
        public void ToggleType_WillSetLoderConfigurationToTypeX_WhenTypeXNotShownCurrently()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loader = new Mock<ILoader>();
            var spyConfig = new LoaderConfiguration();
            var switcher = new SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = spyConfig };

            switcher.ToggleType("a.a");

            Assert.AreEqual("a.a", spyConfig.TypeOfPreviewedGui);
        }

        [TestMethod]
        public void ToggleType_WillShowPreviewOfTypeX_WhenTypeXNotShownCurrently()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var spyLoader = new Mock<ILoader>();
            var config = new LoaderConfiguration();
            var switcher = new SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = mainVM, LoaderConfiguration = config };

            switcher.ToggleType("a.a");

            Assert.AreEqual("a.a", config.TypeOfPreviewedGui);
            spyLoader.Verify(l => l.Show(), Times.Exactly(1));
        }

        [TestMethod]
        public void ToggleType_WontShowPreviewOfTypeX_WhenLoadingHasThrownAnException()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var spyLoader = new Mock<ILoader>();
            var config = new LoaderConfiguration();
            var switcher = new Fake_SwitcherOfLoadedType() { Loader = spyLoader.Object, MainViewModel = mainVM, LoaderConfiguration = config };
            spyLoader.Setup(l => l.LoadType("a.a")).Throws(new Exception());

            switcher.ToggleType("a.a");

            Assert.AreEqual("a.a", config.TypeOfPreviewedGui);
            spyLoader.Verify(l => l.Show(), Times.Never());
        }

        [TestMethod]
        public void ToggleType_WillSetLoderConfigurationFromTypeXToNull_WhenTypeXShownAlready()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loader = new Mock<ILoader>();
            var spyConfig = new LoaderConfiguration();
            var switcher = new SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = spyConfig };

            switcher.ToggleType("b.b");

            Assert.IsNull(spyConfig.TypeOfPreviewedGui);
        }

        [TestMethod]
        public void ToggleType_WillMarkAllTypesUnshown_WhenLoadingHasThrownAnException()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loader = new Mock<ILoader>();
            var spyConfig = new LoaderConfiguration();
            var switcher = new Fake_SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = spyConfig };
            loader.Setup(l => l.LoadType("a.a")).Throws(new Exception());

            switcher.ToggleType("a.a");

            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[0].IsShown);
            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
        }

        [TestMethod]
        public void ToggleType_WillMarkAllTypesUnshown_WhenShowPreviewHasThrownAnException()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b", IsShown = true });
            var loader = new Mock<ILoader>();
            var spyConfig = new LoaderConfiguration();
            var switcher = new Fake_SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = spyConfig };
            loader.Setup(l => l.Show()).Throws(new Exception());

            switcher.ToggleType("a.a");

            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[0].IsShown);
            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
        }

        [TestMethod]
        public void ClosingThePreviewWindowByUser_WillMarkTypeXUnshown_WhenTypeXCurrentlyShown()
        {
            var mainVM = new MainViewModel();
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "a.a" });
            mainVM.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel() { NamespaceDotType = "b.b" });
            var loader = new Mock<ILoader>();
            var spyConfig = new LoaderConfiguration();
            var switcher = new Fake_SwitcherOfLoadedType() { Loader = loader.Object, MainViewModel = mainVM, LoaderConfiguration = spyConfig };

            switcher.ToggleType("a.a");
            switcher.ToggleType("a.a");
            switcher.ToggleType("a.a");
            loader.Raise(l => l.PreviewWindowClosed += null, EventArgs.Empty);

            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[0].IsShown);
            Assert.AreEqual(false, mainVM.ListOfSelectedAssemblyTypes[1].IsShown);
        }
    }
}
