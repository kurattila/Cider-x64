using System.Threading;
using System.Windows.Threading;

namespace Cider_x64
{
    internal interface IWindow
    {
        void Show(double left, double top, double width, double height);
        void Close(AutoResetEvent waitWindowClosedEvent);
        Dispatcher DispatcherInstance { get; }
    }
}
