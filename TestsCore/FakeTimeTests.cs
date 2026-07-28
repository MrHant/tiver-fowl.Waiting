namespace TestsCore
{
    using System.Diagnostics;
    using Fakes;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    /// <summary>
    /// Proves the fake-time plumbing itself before any real test relies on it.
    /// </summary>
    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class FakeTimeTests
    {
        [Test]
        public static void VirtualTimeReplacesWallClockInsideScope()
        {
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(10000, 250);
            var wallClock = Stopwatch.StartNew();

            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => false, config);
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            wallClock.Stop();
            ClassicAssert.IsTrue(success);

            // A full 10 second budget was consumed - in virtual time only.
            ClassicAssert.AreEqual(10000, timer.ElapsedMilliseconds);
            ClassicAssert.Less(wallClock.ElapsedMilliseconds, 5000);
        }

        [Test]
        public static void RealClockIsRestoredAfterScope()
        {
            using (FakeTime.Use(new VirtualWaitTimer()))
            {
                Wait.Until(() => true, new WaitConfiguration(1000, 250));
            }

            var config = new WaitConfiguration(300, 100);
            var wallClock = Stopwatch.StartNew();
            try
            {
                Wait.Until(() => false, config);
            }
            catch (WaitTimeoutException)
            {
            }

            wallClock.Stop();
            ClassicAssert.GreaterOrEqual(wallClock.ElapsedMilliseconds, 300);
        }

        [Test]
        public static void ParkedGateBlocksTheConditionUntilVirtualTimeReachesIt()
        {
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(5000, 100);
            var gate = timer.CreateGate(1200);
            var invocations = 0;

            using (FakeTime.Use(timer))
            {
                var result = Wait.Until(() =>
                {
                    invocations++;
                    gate.Park();
                    return true;
                }, config);

                ClassicAssert.IsTrue(result);
            }

            // One invocation, blocked until exactly the gate's due time.
            ClassicAssert.AreEqual(1, invocations);
            ClassicAssert.AreEqual(1200, timer.ElapsedMilliseconds);
        }

        [Test]
        public static void NeverOpeningGateLetsTheTimeoutExpireExactly()
        {
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(2000, 100);
            var gate = timer.CreateGate();

            var success = false;
            try
            {
                using (FakeTime.Use(timer))
                {
                    Wait.Until(() =>
                    {
                        gate.Park();
                        return true;
                    }, config);
                }
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }
            finally
            {
                gate.Open();
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(2000, timer.ElapsedMilliseconds);
        }

        [Test]
        public static void PollingSleepsAreRecorded()
        {
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(300, 100);

            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => false, config);
                }
                catch (WaitTimeoutException)
                {
                }
            }

            CollectionAssert.AreEqual(new[] { 100, 100, 100 }, timer.RecordedSleeps);
        }
    }
}
