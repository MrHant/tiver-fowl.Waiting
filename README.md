# tiver-fowl.Waiting

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

Custom exit condition
_(Default one - result is not null)_

```c#
var counter = 0;
var result = Wait.Until(() => counter += 1, result => result == 10);
Assert.AreEqual(10, result);
```
