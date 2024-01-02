namespace TestsCore
{
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class ExitConditionTests
    {
        [Test]
        public static void WaitUntilConditionReached()
        {
            var counter = 0;
            var result = Wait.Until(
                () => counter += 1,
                r => r == 10);
            ClassicAssert.AreEqual(10, result);
        }
        
        [Test]
        public static void WaitUntilConditionNotReachedWithTimeout()
        {
            var counter = 0;

            var success = false;
            var config = new WaitConfiguration(500,100);
            try
            {
                Wait.Until(() => counter += 1, r => r == 999, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
        }
    }
}