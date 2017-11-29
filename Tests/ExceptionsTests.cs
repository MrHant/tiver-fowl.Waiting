namespace Tests
{
    using System;
    using Moq;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Exceptions;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class ExceptionsTests
    {
        [Test]
        public static void ExceptionThrown()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());

            var success = false;
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 10);
            }
            catch (ArgumentException)
            {
                success = true;
            }

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void ExceptionIgnoredAndTimeoutIsThrown()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());

            var success = false;
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 10, typeof(ArgumentException));
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

        [Test]
        public static void ExceptionIgnoredAndSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.SetupSequence(foo => foo.GetCount())
                .Throws<ArgumentException>()
                .Throws<ArgumentException>()
                .Throws<ArgumentException>()
                .Returns(4)
                .Returns(5);

            var wait = Wait.Until(() => mock.Object.GetCount() == 5, typeof(ArgumentException));

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(5));
        }
    }
}