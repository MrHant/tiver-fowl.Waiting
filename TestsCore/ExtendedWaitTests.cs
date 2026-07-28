namespace TestsCore
{
    using System;
    using System.Threading;
    using Fakes;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    /// <summary>
    /// The extend-on-timeout feature warns through NUnit, so its scenarios have to run as
    /// nested test cases via <see cref="TestBuilder"/> and report back through statics.
    /// The fixture is deliberately not parallelizable, which keeps those statics safe.
    /// </summary>
    /// <remarks>
    /// Each <c>*Method</c> installs its own <see cref="FakeTime"/> scope rather than
    /// inheriting one from the outer test - the nested case runs on its own NUnit work
    /// item, so an override installed outside it is not something to rely on.
    /// </remarks>
    [TestFixture]
    public static class ExtendedWaitTests
    {
        [Test]
        public static void WaitUntilOneCycleSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var config = new WaitConfiguration(1000, 250, 3000);
            var wait = Wait.Until(() => mock.Object.GetCount() == 10, config);

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        private static int _failingWaitInvocations;
        private static long _failingWaitElapsedMs;
        private static string _failingWaitMessage;

        public static void TotalTimeOfFailingWaitMethod()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(5000, 250, 10000);

            _failingWaitMessage = null;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.GetCount() == 5, config);
                }
                catch (WaitTimeoutException ex)
                {
                    _failingWaitMessage = ex.Message;
                }
            }

            _failingWaitInvocations = mock.Invocations.Count;
            _failingWaitElapsedMs = timer.ElapsedMilliseconds;
        }

        private static int _ignoredExceptionInvocations;
        private static long _ignoredExceptionElapsedMs;
        private static bool _ignoredExceptionTimedOut;

        public static void ExceptionIgnoredViaConfigurationMethod()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());
            var timer = new VirtualWaitTimer();

            _ignoredExceptionTimedOut = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(
                        () => mock.Object.GetCount() == 10,
                        new WaitConfiguration(1000, 250, 5000, typeof(ArgumentException)));
                }
                catch (WaitTimeoutException)
                {
                    _ignoredExceptionTimedOut = true;
                }
            }

            _ignoredExceptionInvocations = mock.Invocations.Count;
            _ignoredExceptionElapsedMs = timer.ElapsedMilliseconds;
        }

        private static int _activeConditionInvocations;
        private static bool _overlapDetected;
        private static long _concurrentElapsedMs;

        public static void ConcurrentConditionInvocationMethod()
        {
            _activeConditionInvocations = 0;
            _overlapDetected = false;
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(500, 100, 2500);

            // The first invocation is still running when the initial timeout expires and
            // the wait is extended - the loop must re-await it rather than start a second.
            var gate = timer.CreateGate(1500);

            try
            {
                using (FakeTime.Use(timer))
                {
                    Wait.Until(() =>
                    {
                        if (Interlocked.Increment(ref _activeConditionInvocations) > 1)
                        {
                            _overlapDetected = true;
                        }

                        try
                        {
                            gate.Park();
                            return false;
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _activeConditionInvocations);
                        }
                    }, config);
                }
            }
            catch (WaitTimeoutException)
            {
            }
            finally
            {
                gate.Open();
            }

            _concurrentElapsedMs = timer.ElapsedMilliseconds;
        }

        [Test]
        public static void ConditionIsNotInvokedConcurrentlyOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(ConcurrentConditionInvocationMethod));

            ClassicAssert.IsFalse(_overlapDetected);
            ClassicAssert.AreEqual(2500, _concurrentElapsedMs);
        }

        private static int _pendingSuccessInvocations;
        private static bool _pendingSuccessResult;
        private static long _pendingSuccessElapsedMs;

        public static void PendingConditionResultUsedMethod()
        {
            _pendingSuccessInvocations = 0;
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(500, 100, 3000);

            // Produces its result after the initial timeout but within the extended one.
            var gate = timer.CreateGate(1200);

            using (FakeTime.Use(timer))
            {
                _pendingSuccessResult = Wait.Until(() =>
                {
                    Interlocked.Increment(ref _pendingSuccessInvocations);
                    gate.Park();
                    return true;
                }, config);
            }

            _pendingSuccessElapsedMs = timer.ElapsedMilliseconds;
        }

        [Test]
        public static void PendingConditionResultIsUsedOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(PendingConditionResultUsedMethod));

            ClassicAssert.IsTrue(_pendingSuccessResult);
            ClassicAssert.AreEqual(1, _pendingSuccessInvocations);

            // The result is taken the moment it appears, not at the extended timeout.
            ClassicAssert.AreEqual(1200, _pendingSuccessElapsedMs);
        }

        private static int _pendingFaultInvocations;
        private static Exception _pendingFaultCaught;
        private static long _pendingFaultElapsedMs;

        public static void PendingConditionExceptionSurfacesMethod()
        {
            _pendingFaultInvocations = 0;
            _pendingFaultCaught = null;
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(500, 100, 3000);
            var gate = timer.CreateGate(1200);

            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until<bool>(() =>
                    {
                        Interlocked.Increment(ref _pendingFaultInvocations);
                        gate.Park();
                        throw new ArgumentException("thrown by pending condition");
                    }, config);
                }
                catch (ArgumentException ex)
                {
                    _pendingFaultCaught = ex;
                }
            }

            _pendingFaultElapsedMs = timer.ElapsedMilliseconds;
        }

        [Test]
        public static void PendingConditionExceptionSurfacesOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(PendingConditionExceptionSurfacesMethod));

            ClassicAssert.IsNotNull(_pendingFaultCaught);
            ClassicAssert.AreEqual(1, _pendingFaultInvocations);
            ClassicAssert.AreEqual(1200, _pendingFaultElapsedMs);
        }

        private static int _recoveryInvocations;
        private static bool _recoveryResult;
        private static long _recoveryElapsedMs;

        public static void NewInvocationSpawnedAfterIgnoredPendingFaultMethod()
        {
            _recoveryInvocations = 0;
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(500, 100, 3000, typeof(ArgumentException));
            var gate = timer.CreateGate(1200);

            using (FakeTime.Use(timer))
            {
                _recoveryResult = Wait.Until(() =>
                {
                    var invocation = Interlocked.Increment(ref _recoveryInvocations);
                    if (invocation == 1)
                    {
                        gate.Park();
                        throw new ArgumentException("first invocation fails slowly");
                    }

                    return true;
                }, config);
            }

            _recoveryElapsedMs = timer.ElapsedMilliseconds;
        }

        [Test]
        public static void NewInvocationSpawnedAfterIgnoredPendingFault()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(NewInvocationSpawnedAfterIgnoredPendingFaultMethod));

            ClassicAssert.IsTrue(_recoveryResult);
            ClassicAssert.AreEqual(2, _recoveryInvocations);

            // Slow failure lands at 1200, the replacement invocation one poll later.
            ClassicAssert.AreEqual(1300, _recoveryElapsedMs);
        }

        [Test]
        public static void TotalTimeOfFailingWait()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                "TotalTimeOfFailingWaitMethod");

            ClassicAssert.AreEqual(ResultState.Warning, result.ResultState);
            ClassicAssert.AreEqual("Timeout for Wait was extended.", result.Message);

            // 20 polls inside the initial 5000 budget, 20 more inside the extension.
            ClassicAssert.AreEqual(40, _failingWaitInvocations);
            ClassicAssert.AreEqual(10000, _failingWaitElapsedMs);
            ClassicAssert.AreEqual(
                "Extended Wait timeout reached after 10000 milliseconds waiting.",
                _failingWaitMessage);
        }

        [Test]
        public static void ExceptionIgnoredViaConfiguration()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                "ExceptionIgnoredViaConfigurationMethod");

            ClassicAssert.AreEqual(ResultState.Warning, result.ResultState);
            ClassicAssert.AreEqual("Timeout for Wait was extended.", result.Message);

            ClassicAssert.IsTrue(_ignoredExceptionTimedOut);
            ClassicAssert.AreEqual(20, _ignoredExceptionInvocations);
            ClassicAssert.AreEqual(5000, _ignoredExceptionElapsedMs);
        }
    }
}
