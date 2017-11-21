namespace Tiver.Fowl.Waiting
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Exceptions;
    using Logging;

    public class Wait
    {
        private static readonly ILog Logger = LogProvider.For<Wait>();

        protected Wait() { }

        public static TResult Until<TResult>(Func<TResult> condition, params Type[] ignoredExceptions)
        {
            IWaitConfiguration config = (WaitConfigurationSection) ConfigurationManager.GetSection("waitConfigurationGroup/waitConfiguration");
            return Until(condition, config, ignoredExceptions);
        }

        public static TResult Until<TResult>(Func<TResult> condition, IWaitConfiguration configuration, params Type[] ignoredExceptions)
        {
            return Until(condition, configuration.Timeout, configuration.PollingInterval, ignoredExceptions);
        }

        private static TResult Until<TResult>(Func<TResult> condition, int timeout, int pollingInterval, params Type[] ignoredExceptions)
        {
            // Start continious checking
            var stopwatch = Stopwatch.StartNew();
            Exception lastException = null;

            while (true)
            {
                // Exit condition - timeout is reached
                CheckTimeoutReached(timeout, stopwatch, lastException);

                try
                {
                    TResult result;
                    try
                    {
                        var task = Task.Factory.StartNew(condition.Invoke);
                        task.Wait(TimeSpan.FromMilliseconds(timeout));
                        result = task.IsCompleted ? task.Result : default(TResult);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.InnerExceptions[0];
                    }

                    // Exit condition - some non-default result
                    if (!EqualityComparer<TResult>.Default.Equals(result, default(TResult)))
                    {
                        using (LogProvider.OpenMappedContext("LogType", "Wait"))
                        {
                            Logger.DebugFormat("Waiting completed in {ms}ms", stopwatch.ElapsedMilliseconds);
                        }

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    var ignored = ignoredExceptions.Any(type => type.IsInstanceOfType(ex));
                    lastException = ex;

                    if (!ignored)
                    {
                        throw;
                    }
                }

                // Exit condition - timeout is reached
                CheckTimeoutReached(timeout, stopwatch, lastException);

                // No exit conditions met - Sleep for polling interval
                Thread.Sleep(pollingInterval);
            }
        }

        private static void CheckTimeoutReached(int timeout, Stopwatch stopwatch, Exception lastException)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            if (elapsedMilliseconds > timeout)
            {
                using (LogProvider.OpenMappedContext("LogType", "Wait"))
                {
                    Logger.DebugFormat("Waiting failed after {ms}ms", elapsedMilliseconds);
                }
                stopwatch.Stop();
                throw new WaitTimeoutException(
                    $"Wait timeout reached after {elapsedMilliseconds} milliseconds waiting.",
                    lastException);
            }
        }
    }
}