namespace TestsCore
{
    using System;
    using System.Runtime.CompilerServices;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
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

            ClassicAssert.IsTrue(success);
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

            ClassicAssert.IsTrue(success);
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

            ClassicAssert.IsTrue(success);
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

            ClassicAssert.IsTrue(success);
            mock.Verify(x => x.GetCount(), Times.AtLeastOnce);
        }


        [Test]
        public static void UnresolvableIgnoredExceptionTypeNameDoesNotHideOriginalException()
        {
            var config = new WaitConfiguration(500, 100)
            {
                IgnoredExceptionsTypeNames = new[] { "Totally.Bogus.TypeName" }
            };

            var success = false;
            try
            {
                Wait.Until<bool>(() => throw new ArgumentException("original failure"), config);
            }
            catch (ArgumentException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
        }

        [Test]
        public static void OriginalStackTraceIsPreservedWhenConditionThrows()
        {
            var config = new WaitConfiguration(500, 100);

            string stackTrace = null;
            try
            {
                Wait.Until(() =>
                {
                    ThrowFromHelper();
                    return true;
                }, config);
            }
            catch (InvalidOperationException ex)
            {
                stackTrace = ex.StackTrace;
            }

            ClassicAssert.IsNotNull(stackTrace);
            ClassicAssert.IsTrue(stackTrace.Contains(nameof(ThrowFromHelper)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFromHelper()
        {
            throw new InvalidOperationException("Thrown from helper");
        }

        [Test]
        public static void LastIgnoredExceptionAvailableAsInnerException()
        {
            var invocations = 0;
            var config = new WaitConfiguration(500, 100, typeof(ArgumentException));

            WaitTimeoutException caught = null;
            try
            {
                Wait.Until<bool>(() =>
                {
                    var invocation = ++invocations;
                    throw new ArgumentException($"failure #{invocation}");
                }, config);
            }
            catch (WaitTimeoutException ex)
            {
                caught = ex;
            }

            ClassicAssert.IsNotNull(caught);
            ClassicAssert.IsInstanceOf<ArgumentException>(caught.InnerException);
            ClassicAssert.AreEqual($"failure #{invocations}", caught.InnerException.Message);
        }

        [Test]
        public static void NoInnerExceptionWhenNoExceptionWasIgnored()
        {
            var config = new WaitConfiguration(500, 100);

            WaitTimeoutException caught = null;
            try
            {
                Wait.Until(() => false, config);
            }
            catch (WaitTimeoutException ex)
            {
                caught = ex;
            }

            ClassicAssert.IsNotNull(caught);
            ClassicAssert.IsNull(caught.InnerException);
        }

        [Test]
        public static void WaitTimeoutExceptionProvidesStandardConstructors()
        {
            ClassicAssert.IsNotNull(typeof(WaitTimeoutException).GetConstructor(Type.EmptyTypes));
            ClassicAssert.IsNotNull(typeof(WaitTimeoutException).GetConstructor(new[] { typeof(string) }));
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

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(3));
        }
    }
}