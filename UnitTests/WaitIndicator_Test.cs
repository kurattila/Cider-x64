using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cider_x64.UnitTests
{
    [TestClass]
    public class WaitIndicator_Test
    {
        [TestMethod]
        public void BeginWaiting_WillShowWindow_Always()
        {
            var waitIndicator = new Fake_WaitIndicator();

            waitIndicator.BeginWaiting(0, 0, 0, 0);

            Assert.IsTrue(waitIndicator.CreatedWindow.CalledShow);
            waitIndicator.EndWaiting();
        }

        [TestMethod]
        public void BeginWaiting_WillShowWindowInSeparateThread_Always()
        {
            var waitIndicator = new Fake_WaitIndicator();
            int callerThreadId = Thread.CurrentThread.ManagedThreadId;

            waitIndicator.BeginWaiting(0, 0, 0, 0);

            Assert.AreEqual(ApartmentState.STA, waitIndicator.CreatedWindow.CalledShowApartmentState);
            Assert.AreNotEqual(callerThreadId, waitIndicator.CreatedWindow.CalledShowThreadId);
            waitIndicator.EndWaiting();
        }

        [TestMethod]
        public void BeginWaiting_WillShowWindowAtSpecifiedSizeAndPosition_Always()
        {
            var waitIndicator = new Fake_WaitIndicator();

            waitIndicator.BeginWaiting(10, 20, 30, 40);

            Assert.AreEqual(10, waitIndicator.CreatedWindow.CalledShowLeft);
            Assert.AreEqual(20, waitIndicator.CreatedWindow.CalledShowTop);
            Assert.AreEqual(30, waitIndicator.CreatedWindow.CalledShowWidth);
            Assert.AreEqual(40, waitIndicator.CreatedWindow.CalledShowHeight);
            waitIndicator.EndWaiting();
        }

        [TestMethod]
        public void EndWaiting_WillCloseWindow_Always()
        {
            var waitIndicator = new Fake_WaitIndicator();
            waitIndicator.BeginWaiting(0, 0, 0, 0);

            waitIndicator.EndWaiting();

            Assert.IsTrue(waitIndicator.CreatedWindow.CalledClose);
        }

        [TestMethod]
        public void EndWaiting_WillCloseWindowInSeparateThread_Always()
        {
            int callerThreadId = Thread.CurrentThread.ManagedThreadId;
            var waitIndicator = new Fake_WaitIndicator();
            waitIndicator.BeginWaiting(0, 0, 0, 0);

            waitIndicator.EndWaiting();

            Assert.AreNotEqual(callerThreadId, waitIndicator.CreatedWindow.CalledCloseThreadId);
        }

        [TestMethod]
        public void EndWaiting_WillAbortBackgroundGuiThread_Always()
        {
            var waitIndicator = new Fake_WaitIndicator();
            waitIndicator.ForcedThreadExecutionMillisecs = 2000;
            waitIndicator.BeginWaiting(0, 0, 0, 0);

            waitIndicator.EndWaiting();

            Assert.IsFalse(waitIndicator.IsThreadRunning);
        }
    }

    class Fake_WaitWindow : IWindow
    {
        public bool CalledClose { get; private set; }
        public int CalledCloseThreadId { get; private set; }
        public void Close(AutoResetEvent waitWindowClosedEvent)
        {
            CalledCloseThreadId = Thread.CurrentThread.ManagedThreadId;
            CalledClose = true;
            waitWindowClosedEvent.Set();
        }

        public bool CalledShow { get; private set; }
        public int CalledShowThreadId { get; private set; }
        public ApartmentState CalledShowApartmentState { get; private set; }
        public double CalledShowLeft { get; private set; }
        public double CalledShowTop { get; private set; }
        public double CalledShowWidth { get; private set; }
        public double CalledShowHeight { get; private set; }
        public void Show(double left, double top, double width, double height)
        {
            CalledShowThreadId = Thread.CurrentThread.ManagedThreadId;
            CalledShowApartmentState = Thread.CurrentThread.GetApartmentState();
            CalledShowLeft = left;
            CalledShowTop = top;
            CalledShowWidth = width;
            CalledShowHeight = height;
            CalledShow = true;

            DispatcherInstance = new System.Windows.Controls.Control().Dispatcher;
        }

        public System.Windows.Threading.Dispatcher DispatcherInstance { get; private set; }
    }

    class Fake_WaitIndicator : WaitIndicator
    {
        public Fake_WaitWindow CreatedWindow
        {
            get { return m_WaitWindow as Fake_WaitWindow; }
        }
        protected override IWindow createWindow()
        {
            return new Fake_WaitWindow();
        }

        public int ForcedThreadExecutionMillisecs = 0;
        protected override void doBackgroundGuiThreadWork()
        {
            base.doBackgroundGuiThreadWork();
            Thread.Sleep(ForcedThreadExecutionMillisecs);
        }

        public bool IsThreadRunning
        {
            get { return IsBackgroundGuiThreadRunning(); }
        }
    }
}
