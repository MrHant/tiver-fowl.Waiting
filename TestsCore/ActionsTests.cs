namespace TestsCore
{
    using System;
    using Fakes;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class ActionsTests
    {
        [Test]
        public static void UntilWithAction()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.Tick()).Verifiable();

            Wait.Until(() => mock.Object.Tick());

            mock.Verify(x => x.Tick(), Times.Once);
        }

        [Test]
        public static void UntilWithActionFailing()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.Tick()).Throws<ArgumentException>().Verifiable();
            var timer = new VirtualWaitTimer();

            // Default configuration - Timeout 1000, PollingInterval 250 - so the
            // ignored exception is swallowed on ticks at 0, 250, 500 and 750.
            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.Tick(), new WaitConfiguration(typeof(ArgumentException)));
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1000, timer.ElapsedMilliseconds);
            mock.Verify(x => x.Tick(), Times.Exactly(4));
        }
    }
}