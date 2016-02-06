using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Windows;

namespace Cider_x64
{
    static class StartupTaskFactory
    {
        public static string NoCheckVersion = "/nocheckversion";
        public static string TestWebRequest = "/testwebrequest";

        public static IStartupTask CreateStartupTask(string[] args)
        {
            if (args.Length > 0 && args[0] == StartupTaskFactory.NoCheckVersion)
            {
                // MessageBox.Show("NullStartupTask");
                return new NullStartupTask();
            }
            else if (args.Length > 0 && args[0] == StartupTaskFactory.TestWebRequest)
            {
                // MessageBox.Show("MockWebRequestStartupTask");
                return new MockWebRequestStartupTask();
            }
            else
            {
                // MessageBox.Show("DefaultStartupTask");
                return new DefaultStartupTask();
            }
        }
    }

    interface IStartupTask
    {
        void Run();
    }

    class DefaultStartupTask : IStartupTask
    {
        // E.g. "http://ubytovanie-vm.hyperlink.sk/Cider-x64-CheckNewVersion.php"
        public string WebServiceUri = Cider_x64.Properties.Settings.Default.CheckNewVersionUri;

        public string MachineName = System.Environment.MachineName;

        public string Version = VersionInfo.AssemblyVersion;

        protected virtual string createConnectionString(string webServiceUri, string machine, string version)
        {
            return string.Format("{0}?computer={1}&version={2}"
                                , webServiceUri
                                , machine
                                , version);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void doWebRequest(HttpWebRequest webRequest)
        {
            HttpWebResponse webResp = null;
            try
            {
                webResp = (HttpWebResponse)webRequest.GetResponse();

                // TODO: Notify user about the new version available
            }
            catch (WebException /*ex*/)
            {
                return;
            }

            // Cleanup
            webResp.Close();
        }

        public virtual void Run()
        {
            var connectionString = createConnectionString(WebServiceUri, MachineName, Version);
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(connectionString);
            WebReq.Method = "GET";
            doWebRequest(WebReq);
        }
    }

    class NullStartupTask : IStartupTask
    {
        public virtual void Run()
        {
            // Do nothing; it's a "null object"
        }
    }

    class MockWebRequestStartupTask : DefaultStartupTask
    {
        public MockWebRequestStartupTask()
        {
            WebServiceUri = "http://ubytovanie-vm.hyperlink.sk/Cider-x64-MockWebRequest.php";
        }
        public override void Run()
        {
            base.Run();
        }
    }
}
