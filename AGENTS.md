# Repository Guidelines

## Project Structure & Module Organization
`Tiver.Fowl.Waiting/` hosts the library, with domain logic in `Wait.cs`, configuration types in `Configuration/`, and custom exceptions under `Exceptions/`. Test suites live in `TestsCore/` (NUnit) and `TestsCoreMsTest/` (MSTest) to ensure cross-framework coverage; each mirrors the public API shape. Shared assets, such as the sample `Tiver_config.json`, sit beside the NUnit tests so they are copied into the output folder during runs. The solution file `Tiver.Fowl.Waiting.sln` ties the projects together for local builds and CI.

## Build, Test, and Development Commands
- `dotnet restore` – fetch solution dependencies defined in the `.csproj` files and `global.json`.
- `dotnet build Tiver.Fowl.Waiting.sln` – compile the library for all target frameworks and confirm it stays warning-clean.
- `dotnet test TestsCore/TestsCore.csproj` – execute the NUnit suite, producing TRX logs compatible with the CI workflow.
- `dotnet test TestsCoreMsTest/TestsCoreMsTest.csproj` – run the MSTest variants that exercise the same scenarios with a different runner.
- `dotnet pack Tiver.Fowl.Waiting/Tiver.Fowl.Waiting.csproj -c Release` – create the NuGet package; tags handled by MinVer set the version.

## Coding Style & Naming Conventions
Target C# 10 with four-space indentation, braces on new lines, and PascalCase for public members. Mirror existing namespace layout (`Tiver.Fowl.Waiting.*`) and keep async-agnostic APIs synchronous unless a feature truly benefits from `Task`. Match file names to contained types (e.g., `WaitConfiguration.cs`).

## Testing Guidelines
Favor NUnit for new coverage unless you must validate behavior that differs between runners. Name fixtures `*Tests.cs`, keeping helper stubs in `TestBuilder.cs` style files. Use `Tiver_config.json` to exercise configuration-driven behavior and ensure polling/timeout values remain small to keep the suite under a few seconds. Maintain deterministic tests—avoid sleeps outside the wait abstraction.

## Commit & Pull Request Guidelines
Follow the existing short, imperative commit style (e.g., “Update devcontainer”). Reference issues in the body when applicable, and group related edits per commit to ease release tagging. Pull requests should include a summary, testing evidence (`dotnet test` output or screenshots for logging), and call out any changes that impact consumers (API adjustments, configuration keys).

## Versioning & Release
Version numbers are computed by MinVer using Git tags. To begin a pre-release, tag the desired semantic version suffix (e.g., `git tag 1.1.0-alpha.0 && git push --tags`). Final releases require tagging `master` or `develop` with the target version; `dotnet pack` will pick the calculated value automatically.
