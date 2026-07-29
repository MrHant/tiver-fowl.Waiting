# tiver-fowl.Waiting [![Test & Publish NuGet package](https://github.com/MrHant/tiver-fowl.Waiting/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MrHant/tiver-fowl.Waiting/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting/)

"Wait" implementation.
Allows to process given condition until timeout is reached.
Overall timeout and polling interval are configurable.
Appearing exceptions can be ignored so processing of condition continues.

## Installation

* Install via NuGet package [Tiver.Fowl.Waiting](https://www.nuget.org/packages/Tiver.Fowl.Waiting/)  

## Configurable

Can be configured via `Tiver_config.json` file in following way:

```json
{
  "Tiver.Fowl.Waiting": {
    "Timeout": 1000,
    "PollingInterval": 250
  }
}
```


Full configuration can look like following:

```json
{
  "Tiver.Fowl.Waiting": {
    "Timeout": 5000,
    "PollingInterval": 250,
    "ExtendOnTimeout": true,
    "ExtendedTimeout": 15000,
    "IgnoredExceptionsTypeNames": [
      "System.ArgumentException",
      "NUnit.Framework.AssertionException, NUnit.Framework"
    ]
  }
}
```

## Loggable

Produces debug log. Uses `Microsoft.Extensions.Logging.Abstractions`

Logger instance can be configured using static method: `Wait.SetLogger(loggerInstance)`

## Timeout Exception

Throws `Tiver.Fowl.Waiting.Exceptions.WaitTimeoutException` on timeout

If exceptions were ignored during the Wait, the last one is available as `InnerException`

## Ignoring  Exceptions

You can ignore exceptions during Wait

Exceptions not listed as ignored propagate immediately with their original stack trace

```c#
// Following code throws System.DivideByZeroException
var zero = 0;
var wait = Wait.Until(() => 2 / zero);

// Following code continue execution before timeout occurs
var zero = 0;
var wait = Wait.Until(() => 2 / zero, new WaitConfiguration(typeof(DivideByZeroException)));
```

If the Wait completes successfully, ignored exceptions are discarded — only on timeout is the last of them reported, as the `InnerException` of the `WaitTimeoutException`

### Type Names Must Be Resolvable

Names configured through `IgnoredExceptionsTypeNames` are resolved with `Type.GetType`. **A name that cannot be resolved is skipped silently** — there is no error and no log entry. The exception it was meant to cover is then not ignored, and it propagates out of `Wait.Until` on the very first attempt.

`Type.GetType` finds unqualified names only for types in the .NET base library. Every other type — from a test framework, a third-party package, or your own project — needs an assembly-qualified name:

```json
{
  "Tiver.Fowl.Waiting": {
    "IgnoredExceptionsTypeNames": [
      "System.ArgumentException",
      "NUnit.Framework.AssertionException, NUnit.Framework",
      "OpenQA.Selenium.StaleElementReferenceException, Selenium.WebDriver"
      "MyApp.Infrastructure.TransientException, MyApp"
    ]
  }
}
```

## Samples

Simple Wait (use `Tiver_config.json` values or defaults)

```c#
var result = Wait.Until(() => 2 + 2);
Assert.AreEqual(4, result);
```

Simple Wait with specific config

```c#
var config = new WaitConfiguration(1000, 250);
var result = Wait.Until(() => 2 + 2, config);
Assert.AreEqual(4, result);
```

Extensible Wait (NUnit only)

```c#
var config = new WaitConfiguration(1000, 250, 5000);
var result = Wait.Until(() => 2 + 2, config);
Assert.AreEqual(4, result);
```

> **Note:** This "extend on timeout" feature works only with NUnit. If you use a different test framework (MSTest, xUnit, etc.), don't enable it — the wait will throw an error instead of running.

Custom exit condition
_(Default one - result is not `default` value of its type, e.g. not `null` for reference types, not `0` for `int`, not `false` for `bool`)_

```c#
var counter = 0;
var result = Wait.Until(() => counter += 1, result => result == 10);
Assert.AreEqual(10, result);
```

## Design decisions

### Synchronous API Rationale

The library intentionally exposes synchronous waiting helpers. They are primarily used in UI integration tests which simulate user interactions.