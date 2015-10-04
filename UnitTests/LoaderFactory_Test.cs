using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting;
using System.Reflection;

namespace Cider_x64.UnitTests
{
    class Fake_LoaderFactory : LoaderFactory
    {
        public AppDomain LoaderDomain
        {
            get { return m_LoaderDomain; }
        }

        public AppDomainSetup AppDomainSetupUsed;
        protected override AppDomain createAppDomainForLoader(AppDomainSetup adSetup)
        {
            AppDomainSetupUsed = adSetup;
            return base.createAppDomainForLoader(adSetup);
        }

        /// <summary>
        /// Modified from http://codedmi.xyz/questions/421376/get-the-appdomain-of-object
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static int GetObjectAppDomainId(object proxy)
        {
            int domainId = 0;

            RealProxy rp = RemotingServices.GetRealProxy(proxy);
            if (rp == null)
                domainId = AppDomain.CurrentDomain.Id;
            else
                domainId = (int)rp.GetType().GetField("_domainID", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rp);

            return domainId;
        }
    }

    [TestClass]
    public class LoaderFactory_Test
    {
        [TestMethod]
        public void Create_WillReturnLoaderInstance_Always()
        {
            var factory = new Fake_LoaderFactory();

            ILoader loader = factory.Create();

            Assert.IsTrue(loader != null);
            AppDomain.Unload(factory.LoaderDomain);
        }

        [TestMethod]
        public void Create_WillCreateSeparateAppDomainWithShadowCopy_Always()
        {
            var factory = new Fake_LoaderFactory();

            ILoader loader = factory.Create();

            Assert.AreEqual("true", factory.AppDomainSetupUsed.ShadowCopyFiles);
            Assert.IsTrue(factory.LoaderDomain != null);
            Assert.IsFalse(factory.LoaderDomain.IsDefaultAppDomain());
            AppDomain.Unload(factory.LoaderDomain);
        }

        [TestMethod]
        public void Create_WillLoadCiderAssemblyIntoSeparateAppDomain_Always()
        {
            var factory = new Fake_LoaderFactory();

            ILoader loader = factory.Create();

            var domainAssemblies = factory.LoaderDomain.GetAssemblies();
            var ciderAssembly = (from assembly in domainAssemblies.AsQueryable()
                                 where assembly.ToString().StartsWith("Cider-x64, Version=")
                                 select assembly).FirstOrDefault();
            Assert.IsTrue(ciderAssembly != null);
            AppDomain.Unload(factory.LoaderDomain);
        }

        [TestMethod]
        public void Create_WillCreateLoaderInsideSeparateAppDomain_Always()
        {
            var factory = new Fake_LoaderFactory();

            ILoader loader = factory.Create();

            int appDomainId = Fake_LoaderFactory.GetObjectAppDomainId(loader);
            Assert.AreEqual(factory.LoaderDomain.Id, appDomainId);
            AppDomain.Unload(factory.LoaderDomain);
        }
    }
}
