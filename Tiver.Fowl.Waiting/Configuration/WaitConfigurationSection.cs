namespace Tiver.Fowl.Waiting.Configuration
{
    using System.Configuration;

    public class WaitConfigurationSection : ConfigurationSection, IWaitConfiguration
    {
        [ConfigurationProperty("timeout", DefaultValue = 1000, IsRequired = true)]
        public int Timeout
        {
            get => (int)this["timeout"];
            set => this["timeout"] = value;
        }

        [ConfigurationProperty("pollingInterval", DefaultValue = 250, IsRequired = true)]
        public int PollingInterval
        {
            get => (int)this["pollingInterval"];
            set => this["pollingInterval"] = value;
        }
    }
}
