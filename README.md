# tiver-fowl.Waiting  [![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/MrHant/tiver-fowl/master/LICENSE)

"Wait" implementation

## Branch status

Branch | Package | Code style | CI
------ | ------- | ---------- | --
master (stable) | [![NuGet](https://img.shields.io/nuget/v/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting/) | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/d62b7b7abc9d4aa9b5f3304b9e0f6af4)](https://www.codacy.com/app/mr.hant/tiver-fowl.Waiting?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MrHant/tiver-fowl.Waiting&amp;utm_campaign=Badge_Grade&amp;branch=master) | [![Build status](https://ci.appveyor.com/api/projects/status/eem0vm70l9o185qv/branch/master?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-waiting/branch/master)
develop | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Tiver.Fowl.Waiting.svg)](https://www.nuget.org/packages/Tiver.Fowl.Waiting) | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/d62b7b7abc9d4aa9b5f3304b9e0f6af4)](https://www.codacy.com/app/mr.hant/tiver-fowl.Waiting?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MrHant/tiver-fowl.Waiting&amp;utm_campaign=Badge_Grade&amp;branch=develop) | [![Build status](https://ci.appveyor.com/api/projects/status/eem0vm70l9o185qv/branch/develop?svg=true)](https://ci.appveyor.com/project/MrHant/tiver-fowl-waiting/branch/develop)

## Configurable

Can be configured via `App.config` file in following way:

    <!-- Configuration section-handler declaration area. -->
    <configSections>
        <sectionGroup name="waitConfigurationGroup">
            <section
                name="waitConfiguration"
                type="Tiver.Fowl.Waiting.Configuration.WaitConfigurationSection, Tiver.Fowl.Waiting"
                allowLocation="true"
                allowDefinition="Everywhere" />
        </sectionGroup>
    </configSections>

    <!-- Configuration section settings area. -->
    <waitConfigurationGroup>
        <waitConfiguration
            timeout="1000"
            pollingInterval="250" />
    </waitConfigurationGroup>


## Loggable

Produces debug log. Doesn't force any specific logging library. (uses LibLog v4.2.6)

You can transparently use any of following loggers:  NLog, Log4Net, EntLib Logging, Serilog and Loupe.

## Exception handling

Throws `Tiver.Fowl.Waiting.Exceptions.WaitTimeoutException` on timeout
 
 
