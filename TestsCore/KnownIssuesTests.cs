namespace TestsCore
{
    using System;
    using System.Runtime.CompilerServices;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;

    /// <summary>
    /// Tests asserting desired behavior for known issues in Wait.
    /// These are expected to fail until the corresponding issues are fixed.
    /// </summary>
    [TestFixture]
    public static class KnownIssuesTests
    {
        // Issue: rethrowing via "throw ae.InnerExceptions[0]" resets the stack trace,
        // losing the frames pointing to the actual failure inside the condition.
        [Test]
        public static void OriginalStackTraceIsPreservedWhenConditionThrows()
        {
            var config = new WaitConfiguration(500, 100);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                Wait.Until(() =>
                {
                    ThrowFromHelper();
                    return true;
                }, config));

            Assert.That(exception.StackTrace, Does.Contain(nameof(ThrowFromHelper)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFromHelper()
        {
            throw new InvalidOperationException("Thrown from helper");
        }
    }
}
