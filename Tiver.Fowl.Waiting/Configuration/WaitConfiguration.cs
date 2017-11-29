namespace Tiver.Fowl.Waiting.Configuration
{
    public class WaitConfiguration : IWaitConfiguration
    {
        public WaitConfiguration()
        {
            Timeout = DefaultValues.Timeout;
            PollingInterval = DefaultValues.PollingInterval;
        }

        public WaitConfiguration(int timeout, int pollingInterval)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
        }

        public WaitConfiguration(int timeout, int pollingInterval, int extendedTimeout)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
            ExtendOnTimeout = true;
            ExtendedTimeout = extendedTimeout;
        }

        public int Timeout { get; }
        public int PollingInterval { get; }
        public bool ExtendOnTimeout { get; }
        public int ExtendedTimeout { get; }
    }
}
