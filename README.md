# tiver-fowl.Waiting

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
