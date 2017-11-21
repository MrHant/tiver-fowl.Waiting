namespace Tests
{
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public class ConfigTests
    {
        [Test]
        public void WaitUntilSuccessEmptyConfigurationSection()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10, new WaitConfigurationSection());

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public void DefaultConfigurationSectionValues()
        {
            var config = new WaitConfigurationSection();
            Assert.AreEqual(1000, config.Timeout);
            Assert.AreEqual(250, config.PollingInterval);
        }
    }
}