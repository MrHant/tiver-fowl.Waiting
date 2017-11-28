using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestsMsTest
{
    using System;
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
    }
}
