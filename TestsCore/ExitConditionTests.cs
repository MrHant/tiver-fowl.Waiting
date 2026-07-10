namespace TestsCore
{
    using System;
    using System.Threading;
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
        public static void DefaultValueOfValueTypeIsNotTreatedAsExitResult()
        {
            var config = new WaitConfiguration(500, 100);

            var success = false;
            try
            {
                Wait.Until(() => 0, config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(success);
        }

        [Test]
        public static void BlockingConditionDoesNotReturnFabricatedDefaultToCustomExit()
        {
            // Regression test for issue #1.
            // The condition blocks past the timeout, so it never produces a real
            // result. The custom exit condition happens to accept default(int) (0).
            // A timed-out blocking condition must surface as a WaitTimeoutException -
            // it must NOT return the fabricated default value that the loop uses
            // internally while the task is still pending.
            var config = new WaitConfiguration(300, 100);

            var success = false;
            try
            {
                Wait.Until(
                    () =>
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        return 42;
                    },
                    result => result == 0,
                    config);
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }

            ClassicAssert.IsTrue(
                success,
                "Blocking condition that never completes must time out, not return a fabricated default value");
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