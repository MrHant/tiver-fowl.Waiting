namespace Tiver.Fowl.Waiting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public class Wait
    {
        private static readonly ILogger Logger = new Logger<Wait>(new NullLoggerFactory());

        protected Wait() { }

        public static TResult Until<TResult>(Func<TResult> condition)
        {
            var waitConfiguration = new WaitConfiguration();

            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting").Bind(waitConfiguration);

            return Until(condition, waitConfiguration);
        }

        public static TResult Until<TResult>(Func<TResult> condition, WaitConfiguration configuration)
        {
            // Start continious checking
            var stopwatch = Stopwatch.StartNew();
            Exception lastException = null;
            var wasExtended = false;
            var currentTimeout = configuration.Timeout;

            while (true)
            {
                // Extend timeout if needed
                if (configuration.ExtendOnTimeout && !wasExtended && NeedToBeExtended(currentTimeout, stopwatch))
                {
                    currentTimeout = configuration.ExtendedTimeout;
                    wasExtended = true;
                    WarnTimeoutWasExtended();
                }

                // Exit condition - timeout is reached
                CheckTimeoutReached(currentTimeout, stopwatch, lastException, wasExtended);

                try
                {
                    TResult result;
                    try
                    {
                        var task = Task.Factory.StartNew(condition.Invoke);
                        task.Wait(TimeSpan.FromMilliseconds(currentTimeout));
                        result = task.IsCompleted ? task.Result : default(TResult);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.InnerExceptions[0];
                    }

                    // Exit condition - some non-default result
                    if (!EqualityComparer<TResult>.Default.Equals(result, default(TResult)))
                    {
                        using (Logger.BeginScope(new Dictionary<string, object> { {"LogType", "Wait" } }))
                        {
                            Logger.Log(LogLevel.Debug, "Waiting completed in {ms}ms", stopwatch.ElapsedMilliseconds);
                        }

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    var ignored = configuration.IgnoredExceptions.Any(type => type.IsInstanceOfType(ex));
                    lastException = ex;

                    if (!ignored)
                    {
                        throw;
                    }
                }

                // Extend timeout if needed
                if (configuration.ExtendOnTimeout && !wasExtended && NeedToBeExtended(currentTimeout, stopwatch))
                {
                    currentTimeout = configuration.ExtendedTimeout;
                    wasExtended = true;
                    WarnTimeoutWasExtended();
                }

                // Exit condition - timeout is reached
                CheckTimeoutReached(currentTimeout, stopwatch, lastException, wasExtended);

                // No exit conditions met - Sleep for polling interval
                Thread.Sleep(configuration.PollingInterval);
            }
        }

        private static void CheckTimeoutReached(int timeout, Stopwatch stopwatch, Exception lastException, bool wasExtended)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            if (IsTimeoutReached(timeout, stopwatch))
            {
                using (Logger.BeginScope(new Dictionary<string, object> { {"LogType", "Wait" } }))
                {
                    Logger.Log(LogLevel.Debug, "Waiting failed after {ms}ms", elapsedMilliseconds);
                }
                stopwatch.Stop();

                var waitName = wasExtended ? "Extended Wait" : "Wait";

                throw new WaitTimeoutException(
                    $"{waitName} timeout reached after {elapsedMilliseconds} milliseconds waiting.",
                    lastException);
            }
        }

        private static bool IsTimeoutReached(int timeout, Stopwatch stopwatch)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            return elapsedMilliseconds > timeout;
        }

        private static bool NeedToBeExtended(int timeout, Stopwatch stopwatch)
        {
            if (!NUnitReferenced)
            {
                throw new InvalidOperationException("NUnit Framework must be referenced to use Extend On Timeout feature.");
            }

            return IsTimeoutReached(timeout, stopwatch);
        }

        private static void WarnTimeoutWasExtended()
        {
            var method = AssertType.GetMethod("Warn", new []{typeof(string)});
            method?.Invoke(null, new object[] { "Timeout for Wait was extended." });
        }

        private static readonly Type AssertType = Type.GetType("NUnit.Framework.Assert, nunit.framework");

        private static readonly bool NUnitReferenced = AssertType != null;
    }
}