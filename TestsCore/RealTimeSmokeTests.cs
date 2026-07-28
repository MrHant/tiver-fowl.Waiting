namespace TestsCore
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    /// <summary>
    /// End-to-end coverage of the real <c>StopwatchWaitTimer</c>, which the fake-time
    /// tests deliberately bypass.
    /// </summary>
    /// <remarks>
    /// Bounds are intentionally generous: these prove the wall-clock path works at all,
    /// never that it hits a precise number. Anything needing an exact count or duration
    /// belongs in a fake-time test instead. Timeouts are kept small so the suite stays
    /// quick, and the fixture is non-parallel so a loaded worker pool cannot stretch the
    /// upper bound.
    /// </remarks>
    [TestFixture, NonParallelizable]
    public static class RealTimeSmokeTests
    {
        private const int UpperBoundMilliseconds = 30_000;

        [Test]
        public static void FailingWaitTimesOutOnTheRealClock()
        {
            var config = new WaitConfiguration(300, 100);
            var stopwatch = Stopwatch.StartNew();

            var success = false;
            try
            {
                Wait.Until(() => false, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            stopwatch.Stop();
            ClassicAssert.IsTrue(success);
            ClassicAssert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 300);
            ClassicAssert.Less(stopwatch.ElapsedMilliseconds, UpperBoundMilliseconds);
        }

        [Test]
        public static void BlockingConditionTimesOutOnTheRealClock()
        {
            var invocations = 0;
            var config = new WaitConfiguration(300, 100);
            var stopwatch = Stopwatch.StartNew();

            var success = false;
            try
            {
                Wait.Until(() =>
                {
                    Interlocked.Increment(ref invocations);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    return true;
                }, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            stopwatch.Stop();
            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1, invocations);
            ClassicAssert.GreaterOrEqual(stopwatch.ElapsedMilliseconds, 300);

            // The wait must not sit around for the condition's full 5 seconds.
            ClassicAssert.Less(stopwatch.ElapsedMilliseconds, UpperBoundMilliseconds);
        }

        [Test]
        public static void SuccessfulWaitReturnsOnTheRealClock()
        {
            var polls = 0;
            var config = new WaitConfiguration(UpperBoundMilliseconds, 100);
            var stopwatch = Stopwatch.StartNew();

            var result = Wait.Until(() => Interlocked.Increment(ref polls) >= 3, config);

            stopwatch.Stop();
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(3, polls);
            ClassicAssert.Less(stopwatch.ElapsedMilliseconds, UpperBoundMilliseconds);
        }
    }
}
