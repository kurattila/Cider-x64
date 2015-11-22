using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Windows.Markup;
using System.Reflection;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class MissingPreloadException_Test
    {
        [TestMethod]
        public void GetAdviceForUser_ReturnsIntroTextAsFirstLine_Always()
        {
            var fileNotFoundException = new FileNotFoundException("message about missing styles", "dummyAssemblyStyles.dll");
            var xamlParseException = new XamlParseException("", fileNotFoundException);
            var targetInvocationException = new TargetInvocationException(xamlParseException);
            var missingPreloadException = new MissingPreloadException("", targetInvocationException);

            string adviceForUser = missingPreloadException.GetAdviceForUser();

            string[] lines = adviceForUser.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(MissingPreloadException.IntroPartOfAdvice, lines[0]);
        }

        [TestMethod]
        public void GetAdviceForUser_ReturnsEmptySecondLine_Always()
        {
            var fileNotFoundException = new FileNotFoundException("message about missing styles", "dummyAssemblyStyles.dll");
            var xamlParseException = new XamlParseException("", fileNotFoundException);
            var targetInvocationException = new TargetInvocationException(xamlParseException);
            var missingPreloadException = new MissingPreloadException("", targetInvocationException);

            string adviceForUser = missingPreloadException.GetAdviceForUser();

            string[] lines = adviceForUser.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual("", lines[1]);
        }

        [TestMethod]
        public void GetAdviceForUser_RepeatsInnermostExceptionAsThirdLine_Always()
        {
            var fileNotFoundException = new FileNotFoundException("message about missing styles", "dummyAssemblyStyles.dll");
            var xamlParseException = new XamlParseException("", fileNotFoundException);
            var targetInvocationException = new TargetInvocationException(xamlParseException);
            var missingPreloadException = new MissingPreloadException("", targetInvocationException);

            string adviceForUser = missingPreloadException.GetAdviceForUser();

            string[] lines = adviceForUser.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual("message about missing styles", lines[2]);
        }
    }
}
