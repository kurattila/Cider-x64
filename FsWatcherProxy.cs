using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cider_x64
{
    public interface IFsWatcherProxy
    {
        string Path { get; set; }
        void SetChangedHandler(FileSystemEventHandler handler);
        bool EnableRaisingEvents { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class FsWatcherProxy : IFsWatcherProxy
                         , IDisposable
    {
        FileSystemWatcher _fsWatcher = new FileSystemWatcher();

        public bool EnableRaisingEvents
        {
            get{ return _fsWatcher.EnableRaisingEvents; }
            set { _fsWatcher.EnableRaisingEvents = value; }
        }

        public void SetChangedHandler(FileSystemEventHandler handler)
        {
            _fsWatcher.Changed += handler;
        }

        public string Path
        {
            get { return _fsWatcher.Path; }
            set { _fsWatcher.Path = value; }
        }

        public void Dispose()
        {
            _fsWatcher.Dispose();
        }
    }
}
