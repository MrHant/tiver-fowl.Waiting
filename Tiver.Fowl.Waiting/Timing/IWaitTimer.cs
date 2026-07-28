namespace Tiver.Fowl.Waiting.Timing
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Every time-dependent operation performed by <see cref="Wait"/>.
    /// Extracted so tests can drive the wait loop with a virtual clock instead of
    /// real wall-clock time. Production always uses <see cref="StopwatchWaitTimer"/>.
    /// </summary>
    internal interface IWaitTimer
    {
        /// <summary>
        /// Milliseconds elapsed since the timer was created.
        /// </summary>
        long ElapsedMilliseconds { get; }

        /// <summary>
        /// Stops measuring. <see cref="ElapsedMilliseconds"/> keeps its last value.
        /// </summary>
        void Stop();

        /// <summary>
        /// Blocks the calling thread for the given number of milliseconds.
        /// </summary>
        void Sleep(int milliseconds);

        /// <summary>
        /// Waits for <paramref name="task"/> for at most <paramref name="timeout"/>.
        /// Returns <c>true</c> when the task completed, <c>false</c> when the timeout expired
        /// with the task still running.
        /// </summary>
        bool WaitForTask(Task task, TimeSpan timeout);
    }
}
