namespace Tests
{
    using System;
    using System.Configuration;
    using Moq;
    using NUnit.Framework;
    using Tiver.Fowl.Waiting;
    using Tiver.Fowl.Waiting.Configuration;

    [TestFixture, Parallelizable(ParallelScope.All)]
    public static class ConfigTests
    {
        [Test]
        public static void WaitUntilSuccessEmptyConfigurationSection()
        {
            var mock = new Mock<ICounter>();
            mock.Setup(foo => foo.GetCount()).Returns(10);

            var wait = Wait.Until(() => mock.Object.GetCount() == 10, new WaitConfigurationSection());

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void DefaultConfigurationSectionValues()
        {
            var config = new WaitConfigurationSection();
            Assert.AreEqual(1000, config.Timeout);
            Assert.AreEqual(250, config.PollingInterval);
            Assert.AreEqual(false, config.ExtendOnTimeout);
            Assert.AreEqual(5000, config.ExtendedTimeout);
            Assert.AreEqual(0, config.IgnoredExceptions.Length);
        }

        [Test]
        public static void MinimumConfigurationSectionFromAppConfig()
        {
            IWaitConfiguration config = (WaitConfigurationSection) ConfigurationManager.GetSection("waitConfigurationGroup/minimumWaitConfiguration");
            Assert.AreEqual(3000, config.Timeout);
            Assert.AreEqual(1000, config.PollingInterval);
            Assert.AreEqual(false, config.ExtendOnTimeout);
            Assert.AreEqual(5000, config.ExtendedTimeout);
            Assert.AreEqual(0, config.IgnoredExceptions.Length);
        }

        [Test]
        public static void FullConfigurationSectionFromAppConfig()
        {
            IWaitConfiguration config = (WaitConfigurationSection) ConfigurationManager.GetSection("waitConfigurationGroup/fullWaitConfiguration");
            Assert.AreEqual(3000, config.Timeout);
            Assert.AreEqual(1000, config.PollingInterval);
            Assert.AreEqual(true, config.ExtendOnTimeout);
            Assert.AreEqual(7000, config.ExtendedTimeout);
            Assert.AreEqual(2, config.IgnoredExceptions.Length);
            Assert.IsTrue(config.IgnoredExceptions[0] == typeof(ArgumentException));
            Assert.IsTrue(config.IgnoredExceptions[1] == typeof(AssertionException));
        }
    }
}