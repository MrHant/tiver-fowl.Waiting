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
