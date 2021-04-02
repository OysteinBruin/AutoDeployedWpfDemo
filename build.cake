#addin nuget:?package=AutoReleaseTool&version=1.0.2

// ARGUMENTS

var target = Argument("target", "Default");
var solutionPath = Argument<string>("solutionPath");
var buildPath = Argument<string>("buildPath");
var appId  = Argument<string>("appId");
var appVersion = Argument<string>("appVersion");


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
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solutionPath, settings =>
        settings.SetConfiguration("Release"));
});

// TEST - TODO: add test task here
Information("No Unit Tests are added yet.");

//
Task("DownloadPreviousReleaseFiles")
    .IsDependentOn("UnzipAutoReleaseTool")
    .Does(() =>
{
    Information("Downloading previous release files");
    
    FilePath azcopyPath = "./tools/Addins/AutoReleaseTool.1.0.2/lib/net45/Tools/azcopy.exe";
    StartProcess(azcopyPath, new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("copy")
            .Append("https://autodeployedwpfdemo.blob.core.windows.net/releases/")
            .Append("./")
            .Append("--recursive")
    });
});

// PREPARE 

Task("Package")
    .IsDependentOn("DownloadPreviousReleaseFiles")
    .Does(() => 
{
    if (!DirectoryExists("./releases"))
    {
        CreateDirectory("./releases");
    }
    FilePath autoReleasePath = "./tools/Addins/AutoReleaseTool.1.0.2/lib/net45/AutoReleaseTool.exe";
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
