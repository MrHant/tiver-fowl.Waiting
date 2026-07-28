namespace TestsCore.Fakes
{
    using System.Threading;

    /// <summary>
    /// Replaces <c>Thread.Sleep</c> inside a test condition. Parking makes "this
    /// condition is blocked" an observable fact for <see cref="VirtualWaitTimer"/>,
    /// which is what lets the fake clock advance without guessing.
    /// </summary>
    /// <remarks>
    /// A gate created with a due time opens by itself once virtual time reaches it.
    /// A gate created without one never opens on its own - it models a condition that
    /// blocks past the whole timeout, and the test must <see cref="Open"/> it in a
    /// <c>finally</c> so the parked thread pool thread is released.
    /// <para>
    /// Once open a gate stays open, and further <see cref="Park"/> calls pass straight
    /// through. That keeps later invocations of a repeatedly polled condition cheap.
    /// </para>
    /// </remarks>
    internal sealed class VirtualGate
    {
        private readonly VirtualWaitTimer _timer;
        private readonly ManualResetEventSlim _opened = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _resumed = new ManualResetEventSlim(false);

        internal VirtualGate(VirtualWaitTimer timer, long? opensAtVirtualMilliseconds)
        {
            _timer = timer;
            OpensAtVirtualMilliseconds = opensAtVirtualMilliseconds;
        }

        /// <summary>
        /// Virtual time at which the timer opens this gate, or <c>null</c> for a gate
        /// that only the test can open.
        /// </summary>
        internal long? OpensAtVirtualMilliseconds { get; }

        /// <summary>
        /// Blocks the calling condition until the gate is open, telling the timer it is
        /// parked for as long as it waits.
        /// </summary>
        public void Park()
        {
            if (_opened.IsSet)
            {
                return;
            }

            _timer.EnterPark(this);
            try
            {
                _opened.Wait();
            }
            finally
            {
                _timer.ExitPark(this);
                _resumed.Set();
            }
        }

        /// <summary>
        /// Releases a parked condition. Safe to call when nothing is parked, and safe to
        /// call twice - the intended use is an unconditional <c>finally</c> in the test.
        /// </summary>
        public void Open()
        {
            _opened.Set();
        }

        /// <summary>
        /// Opens the gate and blocks until the parked condition has actually resumed.
        /// Without that handshake the timer could observe a stale parked signal and
        /// advance time past the work the condition is about to do.
        /// </summary>
        internal void OpenAndAwaitResume()
        {
            _opened.Set();
            _resumed.Wait();
        }
    }
}
