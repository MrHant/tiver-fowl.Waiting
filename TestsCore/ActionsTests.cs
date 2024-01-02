namespace TestsCore
{
    using System;
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

            var success = false;
            try
            {
                Wait.Until(() => mock.Object.Tick(), new WaitConfiguration(typeof(ArgumentException)));
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.Tick(), Times.AtLeast(3));
        }
    }
}