using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
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
    }
}
