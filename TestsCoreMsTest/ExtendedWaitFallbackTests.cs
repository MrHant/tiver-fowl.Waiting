namespace TestsCoreMsTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;

    [TestClass]
    public class ExtendedWaitFallbackTests
    {
        [TestMethod]
        public void ExceptionOnExtendedWait()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            string exceptionMessage = null;
            try
            {
                var config = new WaitConfiguration(1000, 250, 3000);
                Wait.Until(() => mock.Object.GetCount() == 10, config);
            }
            catch (InvalidOperationException ex)
            {
                exceptionMessage = ex.Message;
            }

            Assert.IsNotNull(exceptionMessage);
            Assert.AreEqual("NUnit Framework must be referenced to use Extend On Timeout feature.", exceptionMessage);
            mock.Verify(x => x.GetCount(), Times.Exactly(0));
        }

        // Issue: ExtendOnTimeout requires NUnit even when the wait succeeds within
        // the base timeout and no extension is ever needed. Desired behavior - only
        // fail when the timeout actually needs to be extended.
        // Conflicts with ExceptionOnExtendedWait above, which pins the current
        // fail-fast behavior and needs updating together with the fix.
        [TestMethod]
        public void ExtendOnTimeoutNotNeededSucceedsWithoutNUnit()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var config = new WaitConfiguration(1000, 250, 3000);
            var wait = Wait.Until(() => mock.Object.GetCount() == 10, config);

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }
    }
}
