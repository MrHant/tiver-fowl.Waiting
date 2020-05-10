namespace Tiver.Fowl.Waiting.Configuration
{
    using System;
    using System.Linq;

    public class WaitConfiguration 
    {
        public WaitConfiguration(params Type[] ignoredExceptions)
        {
            IgnoredExceptionsTypeNames = ignoredExceptions.Select(type => type.AssemblyQualifiedName).ToArray();
        }

        public WaitConfiguration(int timeout, int pollingInterval, params Type[] ignoredExceptions)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
            IgnoredExceptionsTypeNames = ignoredExceptions.Select(type => type.AssemblyQualifiedName).ToArray();
        }

        public WaitConfiguration(int timeout, int pollingInterval, int extendedTimeout, params Type[] ignoredExceptions)
        {
            Timeout = timeout;
            PollingInterval = pollingInterval;
            ExtendOnTimeout = true;
            ExtendedTimeout = extendedTimeout;
            IgnoredExceptionsTypeNames = ignoredExceptions.Select(type => type.AssemblyQualifiedName).ToArray();
        }

        public int Timeout { get; set; } = 1000;
        public int PollingInterval { get; set; } = 250;
        public bool ExtendOnTimeout { get; set; } = false;
        public int ExtendedTimeout { get; set; } = 5000;
        public string[] IgnoredExceptionsTypeNames { get; set; }

        public Type[] IgnoredExceptions
        {
            get
            {
                return IgnoredExceptionsTypeNames.Select(name =>
                    Type.GetType(name)).ToArray();;
            }
        }
    }
}
