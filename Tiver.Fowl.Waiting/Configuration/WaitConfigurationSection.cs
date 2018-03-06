namespace Tiver.Fowl.Waiting.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    public class WaitConfigurationSection : ConfigurationSection, IWaitConfiguration
    {
        [ConfigurationProperty("timeout", DefaultValue = DefaultValues.Timeout, IsRequired = true)]
        public int Timeout
        {
            get => (int)this["timeout"];
            set => this["timeout"] = value;
        }

        [ConfigurationProperty("pollingInterval", DefaultValue = DefaultValues.PollingInterval, IsRequired = true)]
        public int PollingInterval
        {
            get => (int)this["pollingInterval"];
            set => this["pollingInterval"] = value;
        }

        [ConfigurationProperty("extendOnTimeout", DefaultValue = false, IsRequired = false)]
        public bool ExtendOnTimeout
        {
            get => (bool)this["extendOnTimeout"];
            set => this["extendOnTimeout"] = value;
        }

        [ConfigurationProperty("extendedTimeout", DefaultValue = 5000, IsRequired = false)]
        public int ExtendedTimeout
        {
            get => (int)this["extendedTimeout"];
            set => this["extendedTimeout"] = value;
        }

        [ConfigurationProperty("IgnoredExceptions", IsDefaultCollection = true)]
        public IgnoredExceptionsCollection IgnoredExceptionsCollection
        {
            get => (IgnoredExceptionsCollection)this["IgnoredExceptions"];
            set => this["IgnoredExceptions"] = value;
        }

        public Type[] IgnoredExceptions
        {
            get
            {
                var result = new List<Type>();
                foreach (ExceptionConfigElement element in IgnoredExceptionsCollection)
                {
                    result.Add(element.Type);
                }
                return result.ToArray();
            }
        }
    }
}
