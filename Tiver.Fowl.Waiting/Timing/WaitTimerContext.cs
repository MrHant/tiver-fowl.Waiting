namespace Tiver.Fowl.Waiting.Timing
{
    using System;
    using System.Threading;

    /// <summary>
    /// Supplies the <see cref="IWaitTimer"/> used by a wait loop.
    /// </summary>
    /// <remarks>
    /// The override is an <see cref="AsyncLocal{T}"/> rather than a plain static so that
    /// test fixtures running in parallel cannot observe each other's fake clocks - each
    /// gets its own execution context.
    /// <para>
    /// Because the override flows with the execution context, it is also visible inside the
    /// condition delegate (which the wait loop runs on the thread pool). That is harmless:
    /// conditions never consult the timer. It does mean a nested <c>Wait.Until</c> started
    /// from inside a condition would share the outer wait's timer factory - a factory that
    /// returns a fresh timer per call keeps that case well-behaved.
    /// </para>
    /// </remarks>
    internal static class WaitTimerContext
    {
        internal static readonly AsyncLocal<Func<IWaitTimer>> FactoryOverride =
            new AsyncLocal<Func<IWaitTimer>>();

        /// <summary>
        /// Creates a timer that has already started measuring.
        /// </summary>
        internal static IWaitTimer CreateTimer()
        {
            var factory = FactoryOverride.Value;
            return factory == null ? new StopwatchWaitTimer() : factory.Invoke();
        }
    }
}
