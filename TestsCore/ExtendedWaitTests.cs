namespace TestsCore
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

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

        public static void TotalTimeOfFailingWaitMethod()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var success = false;
            var stopwatch = new Stopwatch();
            var config = new WaitConfiguration(5000, 250, 10000);
            stopwatch.Start();
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 5, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            stopwatch.Stop();
            ClassicAssert.IsTrue(success);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            ClassicAssert.IsTrue(passedSeconds > 10000 && passedSeconds - 10000 < 1000);
        }

        public static void ExceptionIgnoredViaConfigurationMethod()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());

            var success = false;
            try
            {
                Wait.Until(
                    () => mock.Object.GetCount() == 10,
                    new WaitConfiguration(1000, 250, 5000, typeof(ArgumentException)));
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

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
                typeof(ExtendedWaitTests),
                nameof(ConcurrentConditionInvocationMethod));

            ClassicAssert.IsFalse(_overlapDetected);
        }

        private static int _pendingSuccessInvocations;
        private static bool _pendingSuccessResult;
        private static long _pendingSuccessElapsedMs;

        public static void PendingConditionResultUsedMethod()
        {
            _pendingSuccessInvocations = 0;
            var config = new WaitConfiguration(500, 100, 3000);
            var stopwatch = Stopwatch.StartNew();

            _pendingSuccessResult = Wait.Until(() =>
            {
                Interlocked.Increment(ref _pendingSuccessInvocations);
                Thread.Sleep(1200);
                return true;
            }, config);

            stopwatch.Stop();
            _pendingSuccessElapsedMs = stopwatch.ElapsedMilliseconds;
        }

        [Test]
        public static void PendingConditionResultIsUsedOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(PendingConditionResultUsedMethod));

            ClassicAssert.IsTrue(_pendingSuccessResult);
            ClassicAssert.AreEqual(1, _pendingSuccessInvocations);
            ClassicAssert.IsTrue(_pendingSuccessElapsedMs < 2500);
        }

        private static int _pendingFaultInvocations;
        private static Exception _pendingFaultCaught;

        public static void PendingConditionExceptionSurfacesMethod()
        {
            _pendingFaultInvocations = 0;
            _pendingFaultCaught = null;
            var config = new WaitConfiguration(500, 100, 3000);

            try
            {
                Wait.Until<bool>(() =>
                {
                    Interlocked.Increment(ref _pendingFaultInvocations);
                    Thread.Sleep(1200);
                    throw new ArgumentException("thrown by pending condition");
                }, config);
            }
            catch (ArgumentException ex)
            {
                _pendingFaultCaught = ex;
            }
        }

        [Test]
        public static void PendingConditionExceptionSurfacesOnExtendedWait()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(PendingConditionExceptionSurfacesMethod));

            ClassicAssert.IsNotNull(_pendingFaultCaught);
            ClassicAssert.AreEqual(1, _pendingFaultInvocations);
        }

        private static int _recoveryInvocations;
        private static bool _recoveryResult;

        public static void NewInvocationSpawnedAfterIgnoredPendingFaultMethod()
        {
            _recoveryInvocations = 0;
            var config = new WaitConfiguration(500, 100, 3000, typeof(ArgumentException));

            _recoveryResult = Wait.Until(() =>
            {
                var invocation = Interlocked.Increment(ref _recoveryInvocations);
                if (invocation == 1)
                {
                    Thread.Sleep(1200);
                    throw new ArgumentException("first invocation fails slowly");
                }

                return true;
            }, config);
        }

        [Test]
        public static void NewInvocationSpawnedAfterIgnoredPendingFault()
        {
            TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                nameof(NewInvocationSpawnedAfterIgnoredPendingFaultMethod));

            ClassicAssert.IsTrue(_recoveryResult);
            ClassicAssert.AreEqual(2, _recoveryInvocations);
        }

        [Test]
        public static void TotalTimeOfFailingWait()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                "TotalTimeOfFailingWaitMethod");

            ClassicAssert.AreEqual(ResultState.Warning, result.ResultState);
            ClassicAssert.AreEqual("Timeout for Wait was extended.", result.Message);
        }

        [Test]
        public static void ExceptionIgnoredViaConfiguration()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                "ExceptionIgnoredViaConfigurationMethod");

            ClassicAssert.AreEqual(ResultState.Warning, result.ResultState);
            ClassicAssert.AreEqual("Timeout for Wait was extended.", result.Message);
        }
    }
}