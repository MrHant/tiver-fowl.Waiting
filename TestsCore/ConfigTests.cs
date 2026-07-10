namespace TestsCore
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
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

            ClassicAssert.IsTrue(wait);
            mock.Verify(x => x.GetCount(), Times.Exactly(1));
        }

        [Test]
        public static void DefaultConfigurationSectionValues()
        {
            var config = new WaitConfiguration();
            ClassicAssert.AreEqual(1000, config.Timeout);
            ClassicAssert.AreEqual(250, config.PollingInterval);
            ClassicAssert.AreEqual(false, config.ExtendOnTimeout);
            ClassicAssert.AreEqual(5000, config.ExtendedTimeout);
            ClassicAssert.AreEqual(0, config.IgnoredExceptions.Length);
        }

        [Test]
        public static void IgnoredExceptionsResolvesConfiguredTypesAndFiltersUnresolvable()
        {
            // Characterization test for issue #4 (IgnoredExceptions re-resolves
            // types via reflection on every read). Pins the observable contract so
            // a future caching optimisation can be proven behaviour-preserving:
            // valid type names resolve, unresolvable names are filtered out, and
            // repeated reads return the same set.
            var config = new WaitConfiguration
            {
                IgnoredExceptionsTypeNames = new[]
                {
                    typeof(ArgumentException).AssemblyQualifiedName,
                    "Totally.Bogus.TypeName"
                }
            };

            var first = config.IgnoredExceptions;
            var second = config.IgnoredExceptions;

            ClassicAssert.AreEqual(1, first.Length);
            ClassicAssert.AreEqual(typeof(ArgumentException), first[0]);

            ClassicAssert.AreEqual(first.Length, second.Length);
            ClassicAssert.AreEqual(first[0], second[0]);
        }

        [Test]
        public static void MinimumConfigurationSectionFromAppConfig()
        {
            var waitConfiguration = new WaitConfiguration();
            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting_minimumWaitConfiguration").Bind(waitConfiguration);
            
            ClassicAssert.AreEqual(3000, waitConfiguration.Timeout);
            ClassicAssert.AreEqual(1000, waitConfiguration.PollingInterval);
            ClassicAssert.AreEqual(false, waitConfiguration.ExtendOnTimeout);
            ClassicAssert.AreEqual(5000, waitConfiguration.ExtendedTimeout);
            ClassicAssert.AreEqual(0, waitConfiguration.IgnoredExceptions.Length);
        }

        [Test]
        public static void FullConfigurationSectionFromAppConfig()
        {
            var waitConfiguration = new WaitConfiguration();
            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting_fullWaitConfiguration").Bind(waitConfiguration);

            ClassicAssert.AreEqual(3000, waitConfiguration.Timeout);
            ClassicAssert.AreEqual(1000, waitConfiguration.PollingInterval);
            ClassicAssert.AreEqual(true, waitConfiguration.ExtendOnTimeout);
            ClassicAssert.AreEqual(7000, waitConfiguration.ExtendedTimeout);
            ClassicAssert.AreEqual(2, waitConfiguration.IgnoredExceptions.Length);
            ClassicAssert.IsTrue(waitConfiguration.IgnoredExceptions[0] == typeof(ArgumentException));
            ClassicAssert.IsTrue(waitConfiguration.IgnoredExceptions[1] == typeof(AssertionException));
        }
    }
}