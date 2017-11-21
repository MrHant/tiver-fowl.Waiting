const string project = "Tiver.Fowl.Waiting";
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solutionFilename = Argument("solutionFilename", project + ".sln");
var testsProject = Argument("testsProject", "Tests");
var testsFile = Argument("testsFile", "Tests.dll");
var projects = Argument("projects", project + ";" + "Tests");

var projectDirectories = projects.Split(';');

DirectoryPath vsLatest  = VSWhereLatest();
var msBuildPath = (vsLatest==null)
                            ? null
                            : vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

GitVersion versionInfo;
string version;

Setup(_ =>
{
    Information("");
    Information(@"    _______ _                      ______            _      __          __   _ _   _             ");
    Information(@"   |__   __(_)                    |  ____|          | |     \ \        / /  (_) | (_)            ");
    Information(@"      | |   ___   _____ _ __      | |__ _____      _| |      \ \  /\  / /_ _ _| |_ _ _ __   __ _ ");
    Information(@"      | |  | \ \ / / _ \ '__|     |  __/ _ \ \ /\ / / |       \ \/  \/ / _` | | __| | '_ \ / _` |");
    Information(@"      | |  | |\ V /  __/ |     _  | | | (_) \ V  V /| |  _     \  /\  / (_| | | |_| | | | | (_| |");
    Information(@"      |_|  |_| \_/ \___|_|    (_) |_|  \___/ \_/\_/ |_| (_)     \/  \/ \__,_|_|\__|_|_| |_|\__, |");
    Information(@"                                                                                            __/ |");
    Information(@"                                                                                           |___/ ");
    Information("");
});

Teardown(_ =>
{
    Information("Finished running tasks.");
});

Task("RestoreNuGetPackages")
    .Does(() =>
{
    Information("Restoring nuget packages for {0}", solutionFilename);
    NuGetRestore("./" + solutionFilename);
});

Task("Clean")
    .IsDependentOn("RestoreNuGetPackages")
    .Does(() =>
{
    Information("Cleaning project directories");
    foreach (var dir in projectDirectories) {
        CleanDirectories("./" + dir + "/bin");
        CleanDirectories("./" + dir + "/obj");
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(() =>
{
    Information("Building {0} with configuration {1}", solutionFilename, configuration);
    MSBuild("./" + solutionFilename, new MSBuildSettings {
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = configuration,
        ToolPath = msBuildPath
    });
});

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./" + testsProject + "/bin/" + configuration + "/" + testsFile, new NUnitSettings {
        ToolPath = "./tools/NUnit.ConsoleRunner/tools/nunit3-console.exe"
    });
});

Task("Version")
    .Does(() =>
{
    GitVersion(new GitVersionSettings{
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer,
    });

    versionInfo = GitVersion(new GitVersionSettings{
        OutputType = GitVersionOutput.Json,
    });
    version = versionInfo.LegacySemVerPadded;
});

Task("CreateNuGetPackage")
    .IsDependentOn("RunUnitTests")
    .Does(() =>
{
    Information("Packing version {0}", version);
    var nuGetPackSettings = new NuGetPackSettings {
        Version = version,
        OutputDirectory = "./package"
    };

    NuGetPack("./package/Package.nuspec", nuGetPackSettings);
});

Task("PushNuGetPackage")
    .IsDependentOn("CreateNuGetPackage")
    .Does(() =>
{
    var package = "./package/" + project + "."  + version +".nupkg";

    NuGetPush(package, new NuGetPushSettings {
        Source = "https://nuget.org/",
        ApiKey = Environment.GetEnvironmentVariable("NuGet_API_KEY")
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
