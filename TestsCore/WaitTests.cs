namespace TestsCore
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;
    using Range = Moq.Range;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class WaitTests
    {
        [Test]
        public static void WaitUntilOneCycleSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10);

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void WaitUntilFiveCyclesSuccess()
        {
            var mock = new Mock<ICounter>();
            var calls = 1;
            mock.Setup(foo => foo.GetCount())
                .Returns(() => calls)
                .Callback(() => calls++);

            var wait = Wait.Until(() => mock.Object.GetCount() == 5);

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(5));
        }

        [Test]
        public static void WaitUntilFailure()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var success = false;
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 5);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

        [Test]
        public static void AboutTenTimesPolled()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var success = false;
            var config = new WaitConfiguration(1000,100);
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 5, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.Between(8, 10, Range.Inclusive));
        }

        [Test]
        public static void PollingIntervalDoesNotOvershootTimeoutOrCauseExtraInvocation()
        {
            // PollingInterval deliberately exceeds Timeout. The sleep must be capped
            // to the remaining timeout budget, and the top-of-loop timeout check must
            // throw before invoking the condition again.
            var invocations = 0;
            var config = new WaitConfiguration(200, 2000);
            var stopwatch = Stopwatch.StartNew();

            var success = false;
            try
            {
                Wait.Until(() =>
                {
                    Interlocked.Increment(ref invocations);
                    return false;
                }, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }
            stopwatch.Stop();

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1, invocations);
            ClassicAssert.Less(stopwatch.ElapsedMilliseconds, 1000);
        }

        [Test]
        public static void OneTimePolled()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var success = false;
            var config = new WaitConfiguration(500,1000);
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 5, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void TotalTimeOfFailingWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var success = false;
            var stopwatch = new Stopwatch();
            var config = new WaitConfiguration(10000,250);
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

        [Test]
        public static void TotalTimeOfSuccessfulWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var stopwatch = new Stopwatch();
            var config = new WaitConfiguration(10000,250);
            stopwatch.Start();
            var wait = Wait.Until(() => mock.Object.GetCount() == 10, config);

            stopwatch.Stop();
            ClassicAssert.IsTrue(wait);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            ClassicAssert.IsTrue(passedSeconds < 1000);
        }

        [Test]
        public static void BlockingConditionInvokedOnlyOnce()
        {
            var invocations = 0;
            var config = new WaitConfiguration(500, 100);

            var success = false;
            try
            {
                Wait.Until(() =>
                {
                    Interlocked.Increment(ref invocations);
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    return false;
                }, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1, invocations);
        }

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
                    // Behave well for most of the budget, then block -
                    // total time must still respect the configured timeout
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

        [Test]
        public static void TotalTimeOfTooLongConditionWait()
        {
            var success = false;
            var stopwatch = new Stopwatch();
            var config = new WaitConfiguration(5000, 250);
            stopwatch.Start();
            try
            {
                Wait.Until(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    return true;
                }, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            stopwatch.Stop();
            ClassicAssert.IsTrue(success);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            ClassicAssert.IsTrue(passedSeconds > 5000 && passedSeconds - 5000 < 1000);
        }
    }
}
