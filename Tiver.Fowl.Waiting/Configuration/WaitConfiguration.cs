namespace Tiver.Fowl.Waiting.Configuration
{
    using System;

    public class WaitConfiguration : IWaitConfiguration
    {
        public WaitConfiguration(params Type[] ignoredExceptions)
        {
            Timeout = DefaultValues.Timeout;
            PollingInterval = DefaultValues.PollingInterval;
            IgnoredExceptions = ignoredExceptions;
        }

        public WaitConfiguration(int timeout, int pollingInterval, params Type[] ignoredExceptions)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
            IgnoredExceptions = ignoredExceptions;
        }

        public WaitConfiguration(int timeout, int pollingInterval, int extendedTimeout, params Type[] ignoredExceptions)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
            ExtendOnTimeout = true;
            ExtendedTimeout = extendedTimeout;
            IgnoredExceptions = ignoredExceptions;
        }

        public int Timeout { get; }
        public int PollingInterval { get; }
        public bool ExtendOnTimeout { get; }
        public int ExtendedTimeout { get; }
        public Type[] IgnoredExceptions { get; }
    }
}
