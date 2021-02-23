#tool "nuget:?package=7-Zip.CommandLine"
#addin "nuget:?package=Cake.7zip"

// ARGUMENTS

var target = Argument("target", "Default");
var buildPath = Argument<string>("buildPath");
var appId  = Argument<string>("appId");
var appVersion = Argument<string>("appVersion", "1.0.0");

// Define directories.

var solution = "./src/AutoDeployedWpfDemo.sln";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

// BUILD

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory(buildPath);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solution, settings =>
        settings.SetConfiguration("Release"));
});

// TEST


// DEPLOYMENT

var appPath = new DirectoryPath("./").MakeAbsolute(Context.Environment);
var autoReleaseLink = "https://versionupdater.blob.core.windows.net/releases/AutoReleasev0.0.1.7z";
var autoReleasArchive = appPath.CombineWithFilePath("AutoReleasev0.0.1.7z");

Task("DownloadAutoRelease")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!DirectoryExists("./AutoRelease"))
    {
        CreateDirectory("./AutoRelease");
    }

    using (var wc = new System.Net.WebClient())
    {
        Information("DownloadAutoRelease");
        DownloadFile(
            "https://versionupdater.blob.core.windows.net/releases/AutoReleasev0.0.1.7z",
            "./AutoRelease/AutoReleasev0.0.1.7z" 
        );
    }

});

Task("UnzipAutoRelease")
    .IsDependentOn("DownloadAutoRelease")
    .Does(() =>
{

    SevenZip(m => m
      .InExtractMode()
      .WithArchive(File("./AutoRelease/AutoReleasev0.0.1.7z"))
      .WithArchiveType(SwitchArchiveType.SevenZip)
      .WithOutputDirectory("./AutoRelease/"));
});

Task("Package")
    .IsDependentOn("UnzipAutoRelease")
    .Does(() => 
{
        FilePath autoReleasePath = "./AutoRelease/AutoRelease.exe";
        StartProcess(autoReleasePath, new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append(buildPath)
            .Append(appId)
            .Append(appVersion)
        });
});


// EXECUTION
Task("Default").IsDependentOn("Package");

RunTarget(target);

