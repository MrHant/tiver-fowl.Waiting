# Changelog

## [0.5.0]

### Added
- Internal timing seam in the new `Tiver.Fowl.Waiting.Timing` namespace, covering all four time-dependent operations of the wait loop
  - `IWaitTimer` abstraction with the production `StopwatchWaitTimer` implementation
  - `WaitTimerContext` swaps implementations through an `AsyncLocal`, keeping parallel fixtures isolated
  - Exposed to `TestsCore` via `InternalsVisibleTo` so timing tests run on a virtual clock with exact poll counts and durations
- `WaitTimeoutException()` and `WaitTimeoutException(string message)` constructors, so the type satisfies the standard exception constructor set
- Virtual clock test infrastructure under `TestsCore/Fakes/` (`VirtualWaitTimer`, `FakeTime`, `VirtualGate`)
- `net8.0` test leg in both suites, so the shipped `netstandard2.0` asset is covered instead of only the modern build
- `Taskfile.yml` with `restore`, `build`, `test-core`, `test-mstest`, `test-netstandard`, `tests`, `pack` and `tag` tasks

### Changed
- **BREAKING**: Migrated to .NET 10 — package now targets `netstandard2.0;net10.0` (was `netstandard2.0;net9.0`)
- Updated `Microsoft.Extensions.*` dependencies to 10.0.9
- Exceptions thrown by a condition now propagate unwrapped and keep their original stack trace, instead of being rethrown from an `AggregateException`
- Documented in the README that "extend on timeout" works only under NUnit
- Replaced GitVersion with MinVer for versioning from Git tags
- Replaced the Nuke build and AppVeyor pipeline with GitHub Actions
- Publishing moved to NuGet Trusted Publishing (no API key in CI)
- `master` is built only on tag pushes; test execution split per target framework
- Updated dependencies

### Fixed
- A condition still running when its wait window elapses is re-awaited on the next poll instead of a second invocation being started alongside it
- The exit condition is evaluated only against an actually computed result; a still-pending invocation no longer surfaces a fabricated `default` value
- `Wait.Until` no longer overshoots its timeout window — the final polling sleep is clamped to the remaining budget and the timeout check now triggers on reaching the limit, not on passing it
- Type names in `IgnoredExceptionsTypeNames` that cannot be resolved are skipped instead of producing `null` entries in `IgnoredExceptions`

### Removed
- Test targets for the older frameworks (`net462`, `net472`, `net48`)
- `appveyor.testlogger` dependency


## [0.4.3-alpha.0]

### Added
- NuGet package metadata and automated package publishing
- Dev container definition with recommended VS Code extensions

### Changed
- Updated Nuke to v7 and refreshed GitVersion
- Pinned the .NET SDK version in CI


## [0.4.1]

### Changed
- Updated .NET SDK and package dependencies

### Removed
- Redundant `coverlet.collector` dependency


## [0.4.0]

### Added
- `Wait.Until` overloads accepting an `exitCondition` parameter


## [0.3.2]

### Added
- `Wait.Until` override accepting an `Action` instead of a `Func<TResult>`


## [0.3.1]

### Added
- `netstandard2.0` target framework, broadening consumer reach to .NET Framework
- Test targets for `net462`, `net472` and `net48`

### Changed
- Updated library dependencies


## [0.3.0]

### Changed
- Updated to .NET 6
- Updated dependencies and test loggers
- README is now included in the NuGet package

### Removed
- Obsolete build files


## [0.2.2]

### Changed
- README badges


## [0.2.1]

### Changed
- Updated target framework to .NET 5
- Build migrated to Nuke with AppVeyor targets
- Symbols package format switched to `snupkg`


## [0.2.0]

### Added
- Configurable logger via `Wait.SetLogger`

### Changed
- Switched projects to .NET Core 3.1
- Reworked CI and NuGet packaging for .NET Core


## [v0.1.4]

### Changed
- **BREAKING**: `IgnoredExceptions` moved from `Wait.Until` parameters into `WaitConfiguration`
- Simplified the set of `Wait.Until` overloads

### Removed
- `Wait.Until` overload made redundant by the simplified set


## [v0.1.3]

### Added
- "Extend on timeout" support, with `ExtendOnTimeout` and `ExtendedTimeout` configuration values
- CI pipeline using Cake, GitVersion and AppVeyor

### Fixed
- Missing configuration handling, including default values for an empty configuration section


## [v0.1.2]

### Added
- Unit test suite

### Changed
- Timeout is checked both before and after each condition execution
- Condition executions running longer than the timeout are stopped

### Removed
- Redundant `WaitTimeoutException` constructors


## Earlier Releases

Initial extraction of the Wait implementation from tiver-fowl:
- `Wait.Until` with a configurable overall timeout and polling interval
- `WaitConfiguration` bound from the `Tiver.Fowl.Waiting` section of `Tiver_config.json`
- `WaitTimeoutException` carrying the last ignored exception as `InnerException`
- Ignoring configured exception types so polling continues
