namespace TestsCore
{
    using System.Threading;
    using Fakes;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;
    using Tiver.Fowl.Waiting.Exceptions;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class WaitTests
    {
        [Test]
        public static void WaitUntilOneCycleSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10);

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void WaitUntilFiveCyclesSuccess()
        {
            var mock = new Mock<ICounter>();
            var calls = 1;
            mock.Setup(foo => foo.GetCount())
                .Returns(() => calls)
                .Callback(() => calls++);

            var wait = Wait.Until(() => mock.Object.GetCount() == 5);

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(5));
        }

        [Test]
        public static void WaitUntilFailure()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();

            // Tiver_config.json sets Timeout to 5000; PollingInterval keeps its
            // default of 250, giving 20 polls before the budget runs out.
            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.GetCount() == 5);
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(5000, timer.ElapsedMilliseconds);
            mock.Verify(x => x.GetCount(), Times.Exactly(20));
        }

        [Test]
        public static void AboutTenTimesPolled()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(1000, 100);

            WaitTimeoutException caught = null;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.GetCount() == 5, config);
                }
                catch (WaitTimeoutException ex)
                {
                    caught = ex;
                }
            }

            ClassicAssert.IsNotNull(caught);
            ClassicAssert.AreEqual(1000, timer.ElapsedMilliseconds);
            StringAssert.Contains("after 1000 milliseconds", caught.Message);

            // Polls at 0, 100, ... 900 - the poll that would land on 1000 is
            // pre-empted by the timeout check at the top of the loop.
            mock.Verify(x => x.GetCount(), Times.Exactly(10));
        }

        [Test]
        public static void PollingIntervalDoesNotOvershootTimeoutOrCauseExtraInvocation()
        {
            // PollingInterval deliberately exceeds Timeout. The sleep must be capped
            // to the remaining timeout budget, and the top-of-loop timeout check must
            // throw before invoking the condition again.
            var invocations = 0;
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(200, 2000);

            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() =>
                    {
                        Interlocked.Increment(ref invocations);
                        return false;
                    }, config);
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1, invocations);
            CollectionAssert.AreEqual(new[] { 200 }, timer.RecordedSleeps);
            ClassicAssert.AreEqual(200, timer.ElapsedMilliseconds);
        }

        [Test]
        public static void OneTimePolled()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(500, 1000);

            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.GetCount() == 5, config);
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(500, timer.ElapsedMilliseconds);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void TotalTimeOfFailingWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(10000, 250);

            var success = false;
            using (FakeTime.Use(timer))
            {
                try
                {
                    Wait.Until(() => mock.Object.GetCount() == 5, config);
                }
                catch (WaitTimeoutException)
                {
                    success = true;
                }
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(10000, timer.ElapsedMilliseconds);
            mock.Verify(x => x.GetCount(), Times.Exactly(40));
        }

        [Test]
        public static void TotalTimeOfSuccessfulWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);
            var timer = new VirtualWaitTimer();
            var config = new WaitConfiguration(10000, 250);

            bool wait;
            using (FakeTime.Use(timer))
            {
                wait = Wait.Until(() => mock.Object.GetCount() == 10, config);
            }

            ClassicAssert.IsTrue(wait);

            // Succeeds on the first poll, so not a single millisecond of the
            // budget is spent.
            ClassicAssert.AreEqual(0, timer.ElapsedMilliseconds);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void BlockingConditionInvokedOnlyOnce()
        {
            var invocations = 0;
            var timer = new VirtualWaitTimer();
            var gate = timer.CreateGate();
            var config = new WaitConfiguration(500, 100);

            var success = false;
            try
            {
                using (FakeTime.Use(timer))
                {
                    Wait.Until(() =>
                    {
                        Interlocked.Increment(ref invocations);
                        gate.Park();
                        return false;
                    }, config);
                }
            }
            catch (WaitTimeoutException)
            {
                success = true;
            }
            finally
            {
                // Release the parked thread pool thread.
                gate.Open();
            }

            ClassicAssert.IsTrue(success);
            ClassicAssert.AreEqual(1, invocations);
            ClassicAssert.AreEqual(500, timer.ElapsedMilliseconds);
        }

        [Test]
        public static void TotalTimeOfWaitWithLateBlockingCondition()
        {
            var timer = new VirtualWaitTimer();
            var gate = timer.CreateGate();
            var config = new WaitConfiguration(2000, 100);

            var success = false;
            try
            {
                using (FakeTime.Use(timer))
                {
                    Wait.Until(() =>
                    {
                        // Behave well for most of the budget, then block -
                        // total time must still respect the configured timeout
                        if (timer.ElapsedMilliseconds > 1500)
                        {
                            gate.Park();
                        }

                        return false;
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

            // Blocks for good at 1600, yet the wait still ends at its timeout.
            ClassicAssert.AreEqual(2000, timer.ElapsedMilliseconds);
        }

        [Test]
        public static void TotalTimeOfTooLongConditionWait()
        {
            var timer = new VirtualWaitTimer();
            var gate = timer.CreateGate();
            var config = new WaitConfiguration(5000, 250);

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
            ClassicAssert.AreEqual(5000, timer.ElapsedMilliseconds);
        }
    }
}
