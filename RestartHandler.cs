using System;
using System.IO;

namespace Cider_x64
{
    class RestartHandler : IDisposable
    {
        string m_WatchedFoler;
        AppRestarter m_AppRestarter;
        IFsWatcherProxy m_FsWatcherProxyToUse;
        public virtual void Init(AppRestarter appRestarter, string watchedFolder, IFsWatcherProxy fsWatcherProxyToUse = null)
        {
            m_AppRestarter = appRestarter;
            m_WatchedFoler = watchedFolder;
            m_FsWatcherProxyToUse = fsWatcherProxyToUse;
            if (m_FsWatcherProxyToUse == null)
                m_FsWatcherProxyToUse = new FsWatcherProxy();

            m_FsWatcherProxyToUse.SetChangedHandler(onFsChangeHandler);
            m_FsWatcherProxyToUse.Path = watchedFolder;
            m_FsWatcherProxyToUse.EnableRaisingEvents = true;
        }
        void onFsChangeHandler(object sender, FileSystemEventArgs args)
        {
            this.OnFsChange(args.FullPath);
        }
        public virtual void OnFsChange(string fsChangeFullPath)
        {
            if (Path.GetDirectoryName(fsChangeFullPath) == m_WatchedFoler)
            {
                if (!_restartPending)
                    RestartNow();
            }
        }
        bool _restartPending;
        public virtual void RestartNow()
        {
            _restartPending = true;
            m_AppRestarter.Restart();
        }

        public virtual void Dispose()
        {
            (m_FsWatcherProxyToUse as IDisposable).Dispose();
        }
    }
}
