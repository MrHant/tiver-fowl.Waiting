namespace TestsMsTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tiver.Fowl.Waiting;

    [TestClass]
    public class WaitTests
    {
        [TestMethod]
        public void WaitUntilOneCycleSuccess()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10);

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [TestMethod]
        public void WaitUntilThreeCyclesSuccess()
        {
            var mock = new Mock<ICounter>();
            var calls = 1;
            mock.Setup(foo => foo.GetCount())
                .Returns(() => calls)
                .Callback(() => calls++);

            var wait = Wait.Until(() => mock.Object.GetCount() == 3);

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(3));
        }
    }
}