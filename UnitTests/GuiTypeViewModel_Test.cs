using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Cider_x64.Helpers;
using Cider_x64.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cider_x64.UnitTests
{
    class FakeGuiTypeViewModel : GuiTypeViewModel
    {
        public IValueConverter ForcedFrameworkElementToWin32CoordsConverterConverter = new FrameworkElementToWin32CoordsConverter();
        public override IValueConverter GetFrameworkElementToWin32CoordsConverter()
        {
            return ForcedFrameworkElementToWin32CoordsConverterConverter;
        }
    }

    [TestClass]
    public class GuiTypeViewModel_Test
    {
        [TestMethod]
        public void CurrentHilitedShowButtonRect_GetsUpdated_WhenUpdateCurrentShowButtonCoordsCommandFired()
        {
            var fe = new FrameworkElement();
            var notifiedProps = new List<string>();
            var vm = new FakeGuiTypeViewModel();
            var converter = new Mock<FrameworkElementToWin32CoordsConverter>();
            converter.Setup(c => c.GetFrameworkElementWin32PixelRect(fe)).Returns(new Rect(20, 30, 100, 110));
            vm.ForcedFrameworkElementToWin32CoordsConverterConverter = converter.Object;
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            notifiedProps.Clear();
            vm.UpdateCurrentShowButtonCoordsCommand.Execute(fe);

            Assert.IsTrue(notifiedProps.Contains("CurrentHilitedShowButtonRect"));
        }

        [TestMethod]
        public void CurrentHilitedShowButtonRect_ReturnsLocationOfHilitedShowButton_WhenUpdateCurrentShowButtonCoordsCommandFired()
        {
            var fe = new FrameworkElement();
            var converterStub = new Mock<FrameworkElementToWin32CoordsConverter>();
            converterStub.Setup(c => c.GetFrameworkElementWin32PixelRect(fe)).Returns(new Rect(20, 30, 100, 110));
            var vm = new FakeGuiTypeViewModel();
            vm.ForcedFrameworkElementToWin32CoordsConverterConverter = converterStub.Object;

            vm.UpdateCurrentShowButtonCoordsCommand.Execute(fe);
            var rect = vm.CurrentHilitedShowButtonRect;

            Assert.AreEqual(new Rect(20, 30, 100, 110), rect);
        }

        [TestMethod]
        public void NamespacePart_WillReturnEmptyString_IfClassNotContainedInAnyNamespace()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "classname" };

            string namespacePart = vm.Namespace;

            Assert.AreEqual("", namespacePart);
        }

        [TestMethod]
        public void NamespacePart_WillReturnNamespacePartOfClassName_IfClassContainedInNamespace()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "nsp.cls" };

            string namespacePart = vm.Namespace;

            Assert.AreEqual("nsp", namespacePart);
        }

        [TestMethod]
        public void NamespacePart_DependsOnChangesOfNamespaceDotType_Always()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "nsp.cls" };
            var notifiedProps = new List<string>();
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            vm.NamespaceDotType = "nsp2.cls2";

            Assert.IsTrue(notifiedProps.Contains("Namespace"));
        }

        [TestMethod]
        public void ClassPart_WillReturnClassName_IfClassNotContainedInAnyNamespace()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "classname" };

            string classPart = vm.Class;

            Assert.AreEqual("classname", classPart);
        }

        [TestMethod]
        public void ClassPart_WillReturnClassPartOfClassName_IfClassContainedInNamespace()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "nsp.cls" };

            string classPart = vm.Class;

            Assert.AreEqual("cls", classPart);
        }

        [TestMethod]
        public void ClassPart_DependsOnChangesOfNamespaceDotType_Always()
        {
            var vm = new GuiTypeViewModel() { NamespaceDotType = "nsp.cls" };
            var notifiedProps = new List<string>();
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            vm.NamespaceDotType = "nsp2.cls2";

            Assert.IsTrue(notifiedProps.Contains("Class"));
        }
    }
}
