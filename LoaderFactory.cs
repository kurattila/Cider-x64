
using System;
using System.Reflection;

namespace Cider_x64
{
    internal class LoaderFactory
    {
        protected AppDomain m_LoaderDomain = null;

        public ILoader Create()
        {
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ShadowCopyFiles = "true"; // not a boolean

            m_LoaderDomain = createAppDomainForLoader(adSetup);
            string dir = m_LoaderDomain.SetupInformation.ApplicationBase;

            string selfAssemblyName = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;

            ILoader loader = (ILoader) m_LoaderDomain.CreateInstanceFromAndUnwrap(selfAssemblyName, "Cider_x64.Loader");
            return loader;
        }

        protected virtual AppDomain createAppDomainForLoader(AppDomainSetup adSetup)
        {
            return AppDomain.CreateDomain("GUI Preview Domain", null, adSetup);
        }
    }
}
