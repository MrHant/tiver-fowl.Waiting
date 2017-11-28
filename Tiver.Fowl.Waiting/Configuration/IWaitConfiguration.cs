namespace Tiver.Fowl.Waiting.Configuration
{
    public interface IWaitConfiguration
    {
        int Timeout { get; }

        int PollingInterval { get; }

        bool ExtendOnTimeout { get; }

        int ExtendedTimeout { get; }
    }
}
