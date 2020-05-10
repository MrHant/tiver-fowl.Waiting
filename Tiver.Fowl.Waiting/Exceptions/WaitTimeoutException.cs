namespace Tiver.Fowl.Waiting.Exceptions
{
    using System;

    [Serializable]
    public class WaitTimeoutException : Exception
    {
        public WaitTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
