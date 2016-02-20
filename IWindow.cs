using System.Threading;
using System.Windows.Threading;

namespace Cider_x64
{
    public interface IWindow
    {
        void Show(IWaitIndicatorAppearance appearance, double left, double top, double width, double height);
        void Close(AutoResetEvent waitWindowClosedEvent);
        Dispatcher DispatcherInstance { get; }
    }
}
