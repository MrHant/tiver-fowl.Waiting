# Repository Guidelines

## Project Structure & Module Organization
`Tiver.Fowl.Waiting/` hosts the library, with domain logic in `Wait.cs`, configuration types in `Configuration/`, and custom exceptions under `Exceptions/`. Test suites live in `TestsCore/` (NUnit) and `TestsCoreMsTest/` (MSTest) to ensure cross-framework coverage; each mirrors the public API shape. Shared assets, such as the sample `Tiver_config.json`, sit beside the NUnit tests so they are copied into the output folder during runs. The solution file `Tiver.Fowl.Waiting.sln` ties the projects together for local builds and CI.

## Build, Test, and Development Commands
Run via [Task](https://taskfile.dev) (see `Taskfile.yml`; installed automatically in the devcontainer):
- `task restore` – fetch solution dependencies defined in the `.csproj` files and `global.json`.
- `task build` – compile the library for all target frameworks and confirm it stays warning-clean.
- `task test-core` – execute the NUnit suite, producing TRX logs compatible with the CI workflow.
- `task test-mstest` – run the MSTest variants that exercise the same scenarios with a different runner.
- `task tests` – run both suites above.
- `task test-netstandard` – run both suites against the `netstandard2.0` asset only.
- `task pack` – create the NuGet package; tags handled by MinVer set the version.

## Coding Style & Naming Conventions
Target C# 10 with four-space indentation, braces on new lines, and PascalCase for public members. Mirror existing namespace layout (`Tiver.Fowl.Waiting.*`) and keep async-agnostic APIs synchronous unless a feature truly benefits from `Task`. Match file names to contained types (e.g., `WaitConfiguration.cs`).

## Testing Guidelines
Both test projects multi-target `net8.0;net10.0`. This is deliberate: the library ships `netstandard2.0` and `net10.0`, and a project reference resolves the *best compatible* asset — so the `net10.0` leg exercises the `net10.0` build while the `net8.0` leg exercises the `netstandard2.0` build that .NET Framework and older .NET consumers actually get. Keep the `net8.0` leg when adding target frameworks, otherwise `netstandard2.0` ships untested. Running the `net8.0` leg locally needs the .NET 8 runtime (installed by the devcontainer via `dotnetRuntimeVersions`).

### Fake time

Timing-sensitive tests run on a virtual clock instead of the wall clock, so poll counts and elapsed times are exact rather than windowed. The library exposes an internal seam — `Tiver.Fowl.Waiting/Timing/IWaitTimer.cs` covers all four time-dependent operations of the wait loop (elapsed, stop, sleep, bounded task wait), and `WaitTimerContext` swaps the implementation through an `AsyncLocal`, which keeps parallel fixtures isolated. Production always gets `StopwatchWaitTimer`; tests reach the seam via the existing `[assembly: InternalsVisibleTo("TestsCore")]`.

Write a timing test with `TestsCore/Fakes/`:

```csharp
var timer = new VirtualWaitTimer();
using (FakeTime.Use(timer))
{
    // Wait.Until(...) - runs on virtual time
}
ClassicAssert.AreEqual(1000, timer.ElapsedMilliseconds);
```

Conventions:
- One `VirtualWaitTimer` per `Wait.Until` call, and always inside a `using` scope — a leaked override would follow an NUnit worker thread into unrelated tests.
- Virtual time advances only on polling sleeps and on timeouts expiring; running a condition is free. A condition that must consume time blocks on a `VirtualGate` from `timer.CreateGate(dueMs)` (opens itself at that virtual time) or `timer.CreateGate()` (only the test opens it).
- **Never** `Thread.Sleep` inside a condition under `FakeTime` — the fake cannot tell a sleeping condition from a hung one and fails the test after a 30 s liveness grace. Use `gate.Park()`.
- Open a never-opening gate in a `finally`, otherwise its thread pool thread stays parked.
- Assert exact numbers. A window assertion under fake time is a sign the test is really a real-time test.

`RealTimeSmokeTests` deliberately stays on the wall clock to cover `StopwatchWaitTimer` end to end, and uses generous bounds only (at least the timeout, well under 30 s). **TestsCoreMsTest** also stays on real time, doubling as smoke coverage of the `netstandard2.0` asset. Put anything needing an exact count or duration in a fake-time test instead.

Favor NUnit for new coverage unless you must validate behavior that differs between runners. Name fixtures `*Tests.cs`, keeping helper stubs in `TestBuilder.cs` style files. Use `Tiver_config.json` to exercise configuration-driven behavior and ensure polling/timeout values remain small to keep the suite under a few seconds. Maintain deterministic tests—avoid sleeps outside the wait abstraction.

## Comment Guidelines
Do not comment on reasoning for change, instead explain what the code is doing and why if it's a necessary context not obvious from the code itself.

## Commit & Pull Request Guidelines
Follow the existing short, imperative commit style (e.g., “Update devcontainer”). Reference issues in the body when applicable, and group related edits per commit to ease release tagging. 

## Versioning & Release
Version numbers are computed by MinVer using Git tags. To begin a pre-release, tag the desired semantic version suffix (e.g., `task tag VERSION=1.1.0-alpha.0`). Final releases require tagging `master` or `develop` with the target version (e.g., `task tag VERSION=1.1.0`); `task pack` will pick the calculated value automatically.
