namespace TestsCore
{
    using System;
    using Moq;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
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
                Wait.Until(() => mock.Object.GetCount() == 10, new WaitConfiguration(typeof(ArgumentException)));
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

        [Test]
        public static void ExceptionIgnoredViaConfigurationFirstConstructor()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());

            var success = false;
            try
            {
                Wait.Until(() => mock.Object.GetCount() == 10, new WaitConfiguration(typeof(ArgumentException)));
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            Assert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }

        [Test]
        public static void ExceptionIgnoredViaConfigurationSecondConstructor()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(() => throw new ArgumentException());

            var success = false;
            try
            {
                Wait.Until(
                    () => mock.Object.GetCount() == 10, 
                    new WaitConfiguration(1000, 250, typeof(ArgumentException)));
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
                .Returns(3);

            var wait = Wait.Until(() => mock.Object.GetCount() == 3, new WaitConfiguration(typeof(ArgumentException)));

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(3));
        }
    }
}