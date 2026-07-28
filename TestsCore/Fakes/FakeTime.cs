namespace TestsCore.Fakes
{
    using System;
    using Tiver.Fowl.Waiting.Timing;

    /// <summary>
    /// Installs a <see cref="VirtualWaitTimer"/> for the wait loops started inside the
    /// returned scope.
    /// </summary>
    /// <remarks>
    /// Always use it with <c>using</c>. NUnit reuses worker threads, so an override left
    /// behind would leak into unrelated tests. The context falls back to the real clock
    /// when nothing is installed, which means a missed reset degrades to real time rather
    /// than to a stale fake clock.
    /// </remarks>
    internal static class FakeTime
    {
        public static IDisposable Use(VirtualWaitTimer timer)
        {
            WaitTimerContext.FactoryOverride.Value = () => timer;
            return new Scope();
        }

        private sealed class Scope : IDisposable
        {
            public void Dispose()
            {
                WaitTimerContext.FactoryOverride.Value = null;
            }
        }
    }
}
