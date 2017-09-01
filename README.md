# tiver-fowl.Waiting [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting/) [![Codacy Badge](https://img.shields.io/codacy/grade/d62b7b7abc9d4aa9b5f3304b9e0f6af4/master.svg)](https://www.codacy.com/app/mr.hant/tiver-fowl.Waiting?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MrHant/tiver-fowl.Waiting&amp;utm_campaign=Badge_Grade) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE)

"Wait" implementation

## Configurable

Can be configured via .config files like

    <waitConfiguration
        timeout="5000"
        pollingInterval="250" />


## Loggable

Produces debug log. Doesn't force any specific logging library. (uses LibLog v4.2.6)

You can transparently use any of following loggers:  NLog, Log4Net, EntLib Logging, Serilog and Loupe.

## Exception handling

Throws `Tiver.Fowl.Waiting.Exceptions.WaitTimeoutException` on timeout
