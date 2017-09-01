namespace Tiver.Fowl.Waiting.Configuration
{
    public class WaitConfiguration : IWaitConfiguration
    {
        public WaitConfiguration(int timeout, int pollingInterval)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
        }

        public int Timeout { get; }
        public int PollingInterval { get; }
    }
}
