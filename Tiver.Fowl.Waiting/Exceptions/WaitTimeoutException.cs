namespace Tiver.Fowl.Waiting.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WaitTimeoutException : Exception, ISerializable
    {
        public WaitTimeoutException()
        {
        }

        public WaitTimeoutException(string message)
            : base(message)
        {
        }

        public WaitTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected WaitTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
