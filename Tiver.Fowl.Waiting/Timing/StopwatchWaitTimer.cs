namespace Tiver.Fowl.Waiting.Timing
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The production timer - a thin wrapper over the BCL primitives the wait loop
    /// used directly before the seam was extracted.
    /// </summary>
    internal sealed class StopwatchWaitTimer : IWaitTimer
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public long ElapsedMilliseconds
        {
            get { return _stopwatch.ElapsedMilliseconds; }
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public bool WaitForTask(Task task, TimeSpan timeout)
        {
            return Task.WaitAny(new[] { task }, timeout) == 0;
        }
    }
}
