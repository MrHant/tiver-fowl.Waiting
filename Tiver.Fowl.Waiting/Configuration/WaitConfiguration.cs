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
        /// <summary>
        /// Names of the exception types to ignore, resolved through <see cref="Type.GetType(string)"/>.
        /// Unqualified names resolve only for types in the .NET base library; any other type
        /// requires an assembly-qualified name such as "Namespace.TypeName, AssemblyName".
        /// </summary>
        public string[] IgnoredExceptionsTypeNames { get; set; }

        /// <summary>
        /// The types resolved from <see cref="IgnoredExceptionsTypeNames"/>. Names that cannot be
        /// resolved are omitted without error, so this array may be shorter than the configured
        /// names, and an exception whose name failed to resolve is not ignored by the Wait.
        /// </summary>
        public Type[] IgnoredExceptions
        {
            get
            {
                return IgnoredExceptionsTypeNames
                    .Select(name => Type.GetType(name))
                    .Where(type => type != null)
                    .ToArray();
            }
        }
    }
}
