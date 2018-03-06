namespace Tiver.Fowl.Waiting.Configuration
{
    using System;

    public interface IWaitConfiguration
    {
        int Timeout { get; }

        int PollingInterval { get; }

        bool ExtendOnTimeout { get; }

        int ExtendedTimeout { get; }

        Type[] IgnoredExceptions { get; }
    }
}
