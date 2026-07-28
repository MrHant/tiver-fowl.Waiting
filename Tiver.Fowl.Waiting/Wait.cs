namespace Tiver.Fowl.Waiting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Timing;

    public static class Wait
    {
        private static ILogger _logger = NullLogger.Instance;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void Until(Action action)
        {
            var waitConfiguration = GetConfigurationFromFile();

            Until(() =>
                {
                    action.Invoke();
                    return true;
                },
                waitConfiguration);
        }

        public static void Until(Action action, WaitConfiguration configuration)
        {
            Until(() =>
                {
                    action.Invoke();
                    return true;
                },
                configuration);
        }
        
        public static TResult Until<TResult>(Func<TResult> condition)
        {
            var waitConfiguration = GetConfigurationFromFile();
            return Until(condition, waitConfiguration);
        }

        public static TResult Until<TResult>(Func<TResult> condition, Func<TResult, bool> exitCondition)
        {
            var waitConfiguration = GetConfigurationFromFile();
            return Until(condition, exitCondition, waitConfiguration);
        }
        
        public static TResult Until<TResult>(Func<TResult> condition, WaitConfiguration configuration)
        {
            var defaultExitCondition = new Func<TResult, bool>((result) =>
                !EqualityComparer<TResult>.Default.Equals(result, default));
            
            return Until(condition, defaultExitCondition, configuration);
        }

        public static TResult Until<TResult>(Func<TResult> condition, Func<TResult, bool> exitCondition, WaitConfiguration configuration)
        {
            // Start continious checking
            var timer = WaitTimerContext.CreateTimer();
            Exception lastException = null;
            var wasExtended = false;
            var currentTimeout = configuration.Timeout;
            Task<TResult> pendingTask = null;

            while (true)
            {
                // Extend timeout if needed
                if (configuration.ExtendOnTimeout && !wasExtended && NeedToBeExtended(currentTimeout, timer))
                {
                    currentTimeout = configuration.ExtendedTimeout;
                    wasExtended = true;
                    WarnTimeoutWasExtended();
                }

                // Exit condition - timeout is reached
                CheckTimeoutReached(currentTimeout, timer, lastException, wasExtended);

                try
                {
                    // Re-await a still-running invocation instead of spawning
                    // a new one running concurrently with it
                    var task = pendingTask ?? Task.Factory.StartNew(condition.Invoke);
                    var remaining = GetRemainingMilliseconds(currentTimeout, timer);
                    var completed = timer.WaitForTask(task, TimeSpan.FromMilliseconds(remaining));

                    if (completed)
                    {
                        pendingTask = null;

                        // Unlike task.Result, GetResult rethrows a condition's exception
                        // unwrapped and with its original stack trace preserved
                        var result = task.GetAwaiter().GetResult();

                        // Exit condition - some non-default result
                        // Evaluated only for an actually produced result - a still-pending
                        // invocation must not surface a fabricated default value
                        if (exitCondition.Invoke(result))
                        {
                            using (_logger.BeginScope(new Dictionary<string, object> { {"LogType", "Wait" } }))
                            {
                                _logger.Log(LogLevel.Debug, "Waiting completed in {ms}ms", timer.ElapsedMilliseconds);
                            }

                            return result;
                        }
                    }
                    else
                    {
                        pendingTask = task;
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
                if (configuration.ExtendOnTimeout && !wasExtended && NeedToBeExtended(currentTimeout, timer))
                {
                    currentTimeout = configuration.ExtendedTimeout;
                    wasExtended = true;
                    WarnTimeoutWasExtended();
                }

                // Exit condition - timeout is reached
                CheckTimeoutReached(currentTimeout, timer, lastException, wasExtended);

                // No exit conditions met - sleep until the next poll without
                // exceeding the overall timeout budget
                var sleepDuration = Math.Min(
                    configuration.PollingInterval,
                    GetRemainingMilliseconds(currentTimeout, timer));
                timer.Sleep((int)sleepDuration);
            }
        }

        private static void CheckTimeoutReached(int timeout, IWaitTimer timer, Exception lastException, bool wasExtended)
        {
            var elapsedMilliseconds = timer.ElapsedMilliseconds;
            if (IsTimeoutReached(timeout, timer))
            {
                using (_logger.BeginScope(new Dictionary<string, object> { {"LogType", "Wait" } }))
                {
                    _logger.Log(LogLevel.Debug, "Waiting failed after {ms}ms", elapsedMilliseconds);
                }
                timer.Stop();

                var waitName = wasExtended ? "Extended Wait" : "Wait";

                throw new WaitTimeoutException(
                    $"{waitName} timeout reached after {elapsedMilliseconds} milliseconds waiting.",
                    lastException);
            }
        }

        private static bool IsTimeoutReached(int timeout, IWaitTimer timer)
        {
            var elapsedMilliseconds = timer.ElapsedMilliseconds;
            return elapsedMilliseconds >= timeout;
        }

        private static long GetRemainingMilliseconds(int timeout, IWaitTimer timer)
        {
            return Math.Max(0L, timeout - timer.ElapsedMilliseconds);
        }

        private static bool NeedToBeExtended(int timeout, IWaitTimer timer)
        {
            if (!NUnitReferenced)
            {
                throw new InvalidOperationException("NUnit Framework must be referenced to use Extend On Timeout feature.");
            }

            return IsTimeoutReached(timeout, timer);
        }

        private static void WarnTimeoutWasExtended()
        {
            var method = AssertType.GetMethod("Warn", new []{typeof(string)});
            method?.Invoke(null, new object[] { "Timeout for Wait was extended." });
        }

        private static WaitConfiguration GetConfigurationFromFile()
        {
            var waitConfiguration = new WaitConfiguration();

            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Waiting").Bind(waitConfiguration);
            return waitConfiguration;
        }
        
        private static readonly Type AssertType = Type.GetType("NUnit.Framework.Assert, nunit.framework");

        private static readonly bool NUnitReferenced = AssertType != null;
    }
}
