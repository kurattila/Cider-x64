using System;
using System.Globalization;
using System.Windows;
using Cider_x64.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class FrameworkElementToWin32CoordsConverter_Test
    {
        [TestMethod]
        public void ConvertBack_WillThrow_Always() // we don't want to detect WPF elements under a given screen coordinate
        {
            bool thrown = false;
            var converter = new FrameworkElementToWin32CoordsConverter();

            try
            {
                converter.ConvertBack(null, typeof(FrameworkElement), null, CultureInfo.InvariantCulture);
            }
            catch(NotImplementedException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void Convert_ReturnsEmptyRect_WhenNoFrameworkElementSpecified()
        {
            var converter = new FrameworkElementToWin32CoordsConverter();

            var rect = converter.Convert(null, typeof(Rect), null, CultureInfo.InvariantCulture);

            Assert.AreEqual(new Rect(), rect);
        }

        [TestMethod]
        public void Convert_ReturnsRect100x110At20X30Y_WhenFrameworkElementIsLocatedThere()
        {
            var dummyFe = new FrameworkElement();
            var stubConverter = new Mock<FrameworkElementToWin32CoordsConverter>();
            stubConverter.Setup(c => c.GetFrameworkElementWin32PixelRect(dummyFe)).Returns(new Rect(20, 30, 100, 110));

            var rect = stubConverter.Object.Convert(dummyFe, typeof(Rect), null, CultureInfo.InvariantCulture);

            Assert.AreEqual(new Rect(20, 30, 100, 110), rect);
        }
    }
}
