namespace TestsCore
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    /// <summary>
    /// Tests asserting desired behavior for known issues in Wait.
    /// These are expected to fail until the corresponding issues are fixed.
    /// </summary>
    [TestFixture]
    public static class KnownIssuesTests
    {
        // Issue: rethrowing via "throw ae.InnerExceptions[0]" resets the stack trace,
        // losing the frames pointing to the actual failure inside the condition.
        [Test]
        public static void OriginalStackTraceIsPreservedWhenConditionThrows()
        {
            var config = new WaitConfiguration(500, 100);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                Wait.Until(() =>
                {
                    ThrowFromHelper();
                    return true;
                }, config));

            Assert.That(exception.StackTrace, Does.Contain(nameof(ThrowFromHelper)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFromHelper()
        {
            throw new InvalidOperationException("Thrown from helper");
        }

        // Issue: the inner task.Wait uses the full timeout instead of the remaining time,
        // so a condition which starts blocking late can stretch the total wait
        // to nearly twice the configured timeout.
        [Test]
        public static void TotalTimeOfWaitWithLateBlockingCondition()
        {
            var config = new WaitConfiguration(2000, 100);
            var stopwatch = Stopwatch.StartNew();

            var success = false;
            try
            {
                Wait.Until(() =>
                {
                    if (stopwatch.ElapsedMilliseconds > 1500)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }

                    return false;
                }, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            stopwatch.Stop();
            ClassicAssert.IsTrue(success);
            var passedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            ClassicAssert.IsTrue(passedMilliseconds > 2000 && passedMilliseconds - 2000 < 1000);
        }

        // Issue: a condition task abandoned after task.Wait times out keeps running,
        // while the loop spawns a new invocation - the condition executes concurrently
        // with itself. Reproducible with ExtendOnTimeout since the loop continues
        // polling after the first task is abandoned.
        private static int _activeConditionInvocations;
        private static bool _overlapDetected;

        public static void ConcurrentConditionInvocationMethod()
        {
            _activeConditionInvocations = 0;
            _overlapDetected = false;
            var config = new WaitConfiguration(500, 100, 2500);

            try
            {
                Wait.Until(() =>
                {
                    if (Interlocked.Increment(ref _activeConditionInvocations) > 1)
                    {
                        _overlapDetected = true;
                    }

                    try
                    {
                        Thread.Sleep(1500);
                        return false;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeConditionInvocations);
                    }
                }, config);
            }
            catch (WaitTimeoutException)
            {
            }
        }

        [Test]
        public static void ConditionIsNotInvokedConcurrentlyOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(KnownIssuesTests),
                nameof(ConcurrentConditionInvocationMethod));

            ClassicAssert.IsFalse(_overlapDetected);
        }

        // Issue: WaitTimeoutException lacks the standard exception constructors
        // (parameterless and message-only).
        [Test]
        public static void WaitTimeoutExceptionProvidesStandardConstructors()
        {
            ClassicAssert.IsNotNull(typeof(WaitTimeoutException).GetConstructor(Type.EmptyTypes));
            ClassicAssert.IsNotNull(typeof(WaitTimeoutException).GetConstructor(new[] { typeof(string) }));
        }
    }
}
