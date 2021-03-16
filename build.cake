#tool "nuget:?package=7-Zip.CommandLine"
#addin "nuget:?package=Cake.7zip"
#addin nuget:?package=Cake.AzCopy


// ARGUMENTS

var target = Argument("target", "Default");
var solutionPath Argument<string>("solutionPath");
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

// TEST


// DEPLOYMENT

Task("DownloadPreviousReleaseFiles")
    //.IsDependentOn("Build")
    .Does(() =>
{
    Information("Creating folder releases, downloading previous release files to releases");
    if (!DirectoryExists("./releases"))
    {
        CreateDirectory("./releases");
    }


    AzCopy("https://myaccount.blob.core.windows.net/mycontainer/", "./releases");
        
    foreach(var file in GetFiles("./releases/**"))
    {
        Information(file.Path.GetFilename());
    }


});



var appPath = new DirectoryPath("./").MakeAbsolute(Context.Environment);
var autoReleaseLink = "https://versionupdater.blob.core.windows.net/releases/AutoReleaseTool_v0.0.1.7z";
var autoReleasArchive = appPath.CombineWithFilePath("AutoReleaseTool_v0.0.1.7z");

Task("DownloadAutoRelease")
    .IsDependentOn("DownloadPreviousReleaseFiles")
    .Does(() =>
{
    if (!DirectoryExists("./AutoReleaseTool"))
    {
        CreateDirectory("./AutoReleaseTool");
    }

    using (var wc = new System.Net.WebClient())
    {
        Information("DownloadAutoRelease");
        DownloadFile(
            "https://versionupdater.blob.core.windows.net/releases/AutoReleaseTool_v0.0.1.7z",
            "./AutoRelease/AutoReleaseTool_v0.0.1.7z" 
        );
    }

});




Task("UnzipAutoReleaseTool")
    .IsDependentOn("DownloadAutoReleaseTool")
    .Does(() =>
{

    SevenZip(m => m
      .InExtractMode()
      .WithArchive(File("./AutoReleaseTool/AutoReleaseTool_v0.0.1.7z"))
      .WithArchiveType(SwitchArchiveType.SevenZip)
      .WithOutputDirectory("./AutoReleaseTool/"));
});

Task("Package")
    .IsDependentOn("UnzipAutoReleaseTool")
    .Does(() => 
{
        FilePath autoReleasePath = "./AutoReleaseTool/AutoReleaseTool.exe";
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

