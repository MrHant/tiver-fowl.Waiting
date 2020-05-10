namespace TestsCore
{
    using System;
    using Microsoft.Extensions.Configuration;
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

            var wait = Wait.Until(() => mock.Object.GetCount() == 10, new WaitConfiguration());

            Assert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void DefaultConfigurationSectionValues()
        {
            var config = new WaitConfiguration();
            Assert.AreEqual(1000, config.Timeout);
            Assert.AreEqual(250, config.PollingInterval);
            Assert.AreEqual(false, config.ExtendOnTimeout);
            Assert.AreEqual(5000, config.ExtendedTimeout);
            Assert.AreEqual(0, config.IgnoredExceptions.Length);
        }

        [Test]
        public static void MinimumConfigurationSectionFromAppConfig()
        {
            var waitConfiguration = new WaitConfiguration();
            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting_minimumWaitConfiguration").Bind(waitConfiguration);
            
            Assert.AreEqual(3000, waitConfiguration.Timeout);
            Assert.AreEqual(1000, waitConfiguration.PollingInterval);
            Assert.AreEqual(false, waitConfiguration.ExtendOnTimeout);
            Assert.AreEqual(5000, waitConfiguration.ExtendedTimeout);
            Assert.AreEqual(0, waitConfiguration.IgnoredExceptions.Length);
        }

        [Test]
        public static void FullConfigurationSectionFromAppConfig()
        {
            var waitConfiguration = new WaitConfiguration();
            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting_fullWaitConfiguration").Bind(waitConfiguration);

            Assert.AreEqual(3000, waitConfiguration.Timeout);
            Assert.AreEqual(1000, waitConfiguration.PollingInterval);
            Assert.AreEqual(true, waitConfiguration.ExtendOnTimeout);
            Assert.AreEqual(7000, waitConfiguration.ExtendedTimeout);
            Assert.AreEqual(2, waitConfiguration.IgnoredExceptions.Length);
            Assert.IsTrue(waitConfiguration.IgnoredExceptions[0] == typeof(ArgumentException));
            Assert.IsTrue(waitConfiguration.IgnoredExceptions[1] == typeof(AssertionException));
        }
    }
}