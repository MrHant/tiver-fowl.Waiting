namespace Tests
{
    using System.Diagnostics;
    using Moq;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public class WaitTests
    {
        [Test]
        public void WaitUntilOneCycleSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10);

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public void WaitUntilFiveCyclesSuccess()
        {
            var mock = new Mock<ICounter>();
            var calls = 1;
            mock.Setup(foo => foo.GetCount())
                .Returns(() => calls)
                .Callback(() => calls++);

            var wait = Wait.Until(() => mock.Object.GetCount() == 5);

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(5));
        }

        [Test]
        public void WaitUntilFailure()
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

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

        [Test]
        public void AboutTenTimesPolled()
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

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.Between(8, 10, Range.Inclusive));
        }

        [Test]
        public void OneTimePolled()
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

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public void TotalTimeOfFailingWait()
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
            Assert.IsTrue(success);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            Assert.IsTrue(passedSeconds > 10000 && passedSeconds - 10000 < 1000);
        }

        [Test]
        public void TotalTimeOfSuccessfulWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var stopwatch = new Stopwatch();
            var config = new WaitConfiguration(10000,250);
            stopwatch.Start();
            var wait = Wait.Until(() => mock.Object.GetCount() == 10, config);

            stopwatch.Stop();
            Assert.IsTrue(wait);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            Assert.IsTrue(passedSeconds < 1000);
        }
    }
}
