using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Cider_x64.Helpers;
using Cider_x64.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cider_x64.UnitTests
{
    class FakeMainViewModel : MainViewModel
    {
        public List<Mock<GuiTypeViewModel>> FakeGuiTypeViewModels = new List<Mock<GuiTypeViewModel>>();
        protected override GuiTypeViewModel createGuiTypeViewModelInstance()
        {
            var fakeVM = new Mock<GuiTypeViewModel>();
            FakeGuiTypeViewModels.Add(fakeVM);
            return fakeVM.Object;
        }
    }

    [TestClass]
    public class MainViewModel_Test
    {
        [TestMethod]
        public void TextualInfoForAssemblyTypes_SaysNoAssemblyLoaded_WhenNoAssemblyLoadedYet()
        {
            var vm = new MainViewModel();

            string infoText = vm.TextualInfoForAssemblyTypes;

            Assert.AreEqual(MainViewModel.NoAssemblyLoadedYet, infoText);
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_SaysNoGuiTypesInAssembly_WhenAssemblyLoadedButGuiTypesListEmpty()
        {
            var vm = new MainViewModel();
            vm.SelectedAssembly = "dummyLoadedAssembly.dll";

            string infoText = vm.TextualInfoForAssemblyTypes;

            Assert.AreEqual(MainViewModel.NoGuiTypesInAssembly, infoText);
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_IsEmptyString_WhenAssemblyLoadedAndGuiTypesListNotEmpty()
        {
            var vm = new MainViewModel();
            vm.SelectedAssembly = "dummyLoadedAssembly.dll";
            vm.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel());

            string infoText = vm.TextualInfoForAssemblyTypes;

            Assert.IsTrue(string.IsNullOrEmpty(infoText));
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_WillBeAutoUpdated_WhenAnAssemblyGetsLoaded()
        {
            var notifiedProps = new List<string>();
            var vm = new MainViewModel();
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            vm.SelectedAssembly = "dummyLoadedAssembly.dll";

            Assert.IsTrue(notifiedProps.Contains("TextualInfoForAssemblyTypes"));
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_WillBeAutoUpdated_WhenGuiTypeAddedToGuiTypesList()
        {
            var notifiedProps = new List<string>();
            var vm = new MainViewModel();
            vm.SelectedAssembly = "dummyLoadedAssembly.dll";
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            vm.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel());

            Assert.IsTrue(notifiedProps.Contains("TextualInfoForAssemblyTypes"));
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_WillBeAutoUpdated_WhenGuiTypesListInstanceReplaced()
        {
            var notifiedProps = new List<string>();
            var vm = new MainViewModel();
            vm.SelectedAssembly = "dummyLoadedAssembly.dll";
            var newList = new ObservableCollection<ViewModel.GuiTypeViewModel>();
            newList.Add(new ViewModel.GuiTypeViewModel());
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);

            vm.ListOfSelectedAssemblyTypes = newList;

            Assert.IsTrue(notifiedProps.Contains("TextualInfoForAssemblyTypes"));
        }

        [TestMethod]
        public void TextualInfoForAssemblyTypes_WillBeAutoUpdated_WhenGuiTypeAddedToNewInstanceOfGuiTypesListInstance()
        {
            var notifiedProps = new List<string>();
            var vm = new MainViewModel();
            vm.SelectedAssembly = "dummyLoadedAssembly.dll";
            var newList = new ObservableCollection<ViewModel.GuiTypeViewModel>();
            newList.Add(new ViewModel.GuiTypeViewModel());

            vm.ListOfSelectedAssemblyTypes = newList;
            vm.PropertyChanged += (sender, args) => notifiedProps.Add(args.PropertyName);
            vm.ListOfSelectedAssemblyTypes.Add(new ViewModel.GuiTypeViewModel());

            Assert.IsTrue(notifiedProps.Contains("TextualInfoForAssemblyTypes"));
        }

        [TestMethod]
        public void ShowCommand_WillExecuteChangeTypeCommand_Always()
        {
            bool calledChangeTypeCommand = false;
            var vm = new MainViewModel();
            vm.SetWaitIndicator(new Mock<WaitIndicator>().Object);
            vm.InitWithGuiTypes(new List<string>() { "A.a", "B.b", "C.c" });
            var spyCommand = new Mock<ICommand>();
            spyCommand.Setup(c => c.Execute("A.a")).Callback(() => calledChangeTypeCommand = true);
            vm.ChangeTypeCommand = spyCommand.Object;

            vm.ListOfSelectedAssemblyTypes[0].ShowCommand.Execute("A.a");

            Assert.IsTrue(calledChangeTypeCommand);
        }

        [TestMethod]
        public void ShowCommand_WillDisplayWaitIndicator_Always()
        {
            bool calledWaitIndicatorBegin = false;
            var spyWaitIndicator = new Mock<WaitIndicator>();
            var dummyAppearance = new Mock<IWaitIndicatorAppearance>();
            spyWaitIndicator.Setup(w => w.BeginWaiting(dummyAppearance.Object, 0, 0, 0, 0)).Callback(() => calledWaitIndicatorBegin = true);
            var vm = new MainViewModel();
            vm.PlayButtonWaitIndicatorAppearance = dummyAppearance.Object;
            vm.SetWaitIndicator(spyWaitIndicator.Object);
            vm.InitWithGuiTypes(new List<string>() { "A.a", "B.b", "C.c" });
            var dummyCommand = new Mock<ICommand>();
            vm.ChangeTypeCommand = dummyCommand.Object;

            vm.ListOfSelectedAssemblyTypes[0].ShowCommand.Execute("A.a");

            Assert.IsTrue(calledWaitIndicatorBegin);
        }

        [TestMethod]
        public void ShowCommand_WillPlaceWaitIndicatorProperly_Always()
        {
            bool waitIndicatorPlacementCorrect = false;
            var dummyFe = new FrameworkElement();
            var spyWaitIndicator = new Mock<WaitIndicator>();
            var dummyAppearance = new Mock<IWaitIndicatorAppearance>();
            spyWaitIndicator.Setup(w => w.BeginWaiting(dummyAppearance.Object, 20, 30, 100, 110)).Callback(() => waitIndicatorPlacementCorrect = true);
            var vm = new FakeMainViewModel();
            vm.PlayButtonWaitIndicatorAppearance = dummyAppearance.Object;
            vm.SetWaitIndicator(spyWaitIndicator.Object);
            vm.InitWithGuiTypes(new List<string>() { "A.a", "B.b", "C.c" });
            var dummyCommand = new Mock<ICommand>();
            vm.ChangeTypeCommand = dummyCommand.Object;
            var stubConverter = new Mock<FrameworkElementToWin32CoordsConverter>();
            stubConverter.Setup(c => c.GetFrameworkElementWin32PixelRect(dummyFe)).Returns(new Rect(20, 30, 100, 110));
            vm.FakeGuiTypeViewModels[1].Setup(guiTypeVm => guiTypeVm.GetFrameworkElementToWin32CoordsConverter()).Returns(stubConverter.Object);
            vm.FakeGuiTypeViewModels[1].Object.UpdateCurrentShowButtonCoordsCommand.Execute(dummyFe);

            vm.ListOfSelectedAssemblyTypes[0].ShowCommand.Execute("B.b");

            Assert.IsTrue(waitIndicatorPlacementCorrect);
        }

        [TestMethod]
        public void ShowCommand_WillCloseWaitIndicator_AfterChangeTypeCommandFinished()
        {
            bool calledWaitIndicatorEnd = false;
            var spyWaitIndicator = new Mock<WaitIndicator>();
            spyWaitIndicator.Setup(w => w.EndWaiting()).Callback(() => calledWaitIndicatorEnd = true);
            var vm = new MainViewModel();
            vm.SetWaitIndicator(spyWaitIndicator.Object);
            vm.InitWithGuiTypes(new List<string>() { "A.a", "B.b", "C.c" });
            var dummyCommand = new Mock<ICommand>();
            vm.ChangeTypeCommand = dummyCommand.Object;

            vm.ListOfSelectedAssemblyTypes[0].ShowCommand.Execute("A.a");

            Assert.IsTrue(calledWaitIndicatorEnd);
        }
    }
}
