namespace Tiver.Fowl.Waiting.Configuration
{
    using System.Configuration;

    public class WaitConfigurationSection : ConfigurationSection, IWaitConfiguration
    {
        public int Timeout
        {
            get
            {
                return this.TimeoutElement;
            }
        }

        public int PollingInterval
        {
            get
            {
                return this.PollingIntervalElement;
            }
        }

        [ConfigurationProperty("timeout", DefaultValue = 1000, IsRequired = true)]
        private int TimeoutElement
        {
            get
            {
                return (int)this["timeout"];
            }

            set
            {
                this["timeout"] = value;
            }
        }

        [ConfigurationProperty("pollingInterval", DefaultValue = 250, IsRequired = true)]
        private int PollingIntervalElement
        {
            get
            {
                return (int)this["pollingInterval"];
            }

            set
            {
                this["pollingInterval"] = value;
            }
        }
    }
}
