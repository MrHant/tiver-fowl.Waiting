namespace TestsCore
{
    using System;
    using System.Diagnostics;
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