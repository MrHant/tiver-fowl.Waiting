namespace Tests
{
    using System.Diagnostics;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using Tests;
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

            Assert.IsTrue(wait);
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
            Assert.IsTrue(success);
            var passedSeconds = stopwatch.Elapsed.TotalMilliseconds;
            Assert.IsTrue(passedSeconds > 10000 && passedSeconds - 10000 < 1000);
        }

        [Test]
        public static void TotalTimeOfFailingWait()
        {
            ITestResult result = TestBuilder.RunTestCase(
                typeof(ExtendedWaitTests),
                "TotalTimeOfFailingWaitMethod");

            Assert.AreEqual(ResultState.Warning, result.ResultState);
            Assert.AreEqual("Timeout for Wait was extended.", result.Message);
        }
    }
}