# tiver-fowl.Waiting  ![.NET](https://img.shields.io/badge/.NET-6-blue) [![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE) [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FMrHant%2Ftiver-fowl.Waiting.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FMrHant%2Ftiver-fowl.Waiting?ref=badge_shield)

"Wait" implementation.
Allows to process given condition until timeout is reached.
Overall timeout and polling interval are configurable.
Appearing exceptions can be ignored so processing of condition continues.

## Branch status

| Branch | Package | CI  |
| ------ | ------- | --- |
| master (stable) | [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting/) | [![Build status](https://ci.appveyor.com/api/projects/status/eem0vm70l9o185qv/branch/master?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-waiting/branch/master) |
| develop | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting/absoluteLatest) | [![Build status](https://ci.appveyor.com/api/projects/status/eem0vm70l9o185qv/branch/develop?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-waiting/branch/develop) |

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

## Ignoring  Exceptions

You can ignore exceptions during Wait

```c#
// Following code throws System.DivideByZeroException
var zero = 0;
var wait = Wait.Until(() => 2 / zero);

// Following code continue execution before timeout occurs
var zero = 0;
var wait = Wait.Until(() => 2 / zero, new WaitConfiguration(typeof(DivideByZeroException)));
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

Extensible Wait

```c#
var config = new WaitConfiguration(1000, 250, 5000);
var result = Wait.Until(() => 2 + 2, config);
Assert.AreEqual(4, result);
```
