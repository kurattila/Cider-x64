using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class GuiTypesExtractor_Test
    {
        [TestMethod]
        public void GetTypes_WillReturnEmptyCollection_WhenAssemblyWithNoTypesInsidedSpecified()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(0, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillReturnSingleType_WhenOnlyWindowTypePresentInAssembly()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(Window));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillReturnSingleType_WhenOnlyUserControlTypePresentInAssembly()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(UserControl));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillReturnCustomType_WhenCustomTypeDerivedFromWindow()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(SubTypeOfWindow));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillReturnCustomType_WhenCustomTypeDerivedFromUserControl()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(SubTypeOfUserControl));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillReturnEmptyCollection_WhenOnlyNonGuiTypesInsideAssembly()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(System.IO.Stream));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(System.Net.Sockets.Socket));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(ResourceDictionary));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(0, types.Count);
        }

        [TestMethod]
        public void GetTypes_WillFilterOutNonGuiTypes_WhenBothGuiAndNonGuiTypesInsideAssembly()
        {
            var typesExtractor = new Fake_GuiTypesExtractor();
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(Window));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(Page));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(System.IO.Stream));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(System.Net.Sockets.Socket));
            typesExtractor.ForcedAllTypesInsideAssembly.Add(typeof(ResourceDictionary));

            var types = typesExtractor.GetGuiTypesOnly(new AssemblyWrapper());

            Assert.AreEqual(2, types.Count);
        }

        class Fake_GuiTypesExtractor : GuiTypesExtractor
        {
            public List<Type> ForcedAllTypesInsideAssembly = new List<Type>();
            protected override List<Type> getAllTypesInsideAssembly(AssemblyWrapper assembly)
            {
                return ForcedAllTypesInsideAssembly;
            }
        }

        class SubTypeOfWindow : Window
        { }

        class SubTypeOfUserControl : UserControl
        { }
    }
}
