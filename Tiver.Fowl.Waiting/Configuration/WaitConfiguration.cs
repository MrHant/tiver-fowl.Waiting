namespace Tiver.Fowl.Waiting.Configuration
{
    public class WaitConfiguration : IWaitConfiguration
    {
        public WaitConfiguration(int timeout, int pollingInterval)
        {
            this.Timeout = timeout;
            this.PollingInterval = pollingInterval;
        }

        public int Timeout { get; private set; }

        public int PollingInterval { get; private set; }
    }
}
