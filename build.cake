#addin "Cake.FileHelpers&version=3.2.1"
#load "build_scripts/license.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var docInPath = Argument<string>("doc_path", null) ?? EnvironmentVariable("FPLEDIT_DOK");
var ignoreNoDoc = Argument<string>("ignore_no_doc", null) != null;
var preBuildVersionSuffix = Argument("version_suffix", "");

var incrementVersion = false;

if (Argument<string>("auto-beta", null) != null) {
    ignoreNoDoc = true;
    incrementVersion = true;
    preBuildVersionSuffix = "beta"; //TODO: missing version suffix increment in version info
}

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./bin") + Directory(configuration);
var sourceDir = Directory(".");
var scriptsDir = Directory("./build_scripts");
var tempDir = Directory("./build_tmp");

var doc_generated = false;

if (incrementVersion && string.IsNullOrEmpty(preBuildVersionSuffix))
    throw new InvalidOperationException("No version suffix specified, but incrementVersion!");
if (incrementVersion) 
{
    var currentVersion = XmlPeek(scriptsDir + File("VersionInfo.targets"), "/Project/PropertyGroup/VersionPrefix");
    
    var fn = $"fpledit-{currentVersion}-{preBuildVersionSuffix}";
    
    if (incrementVersion) {
        int counter = 1;
        while (GetFiles(Directory("./bin") + File($"{fn}{counter}*.zip")).Any()) {
            counter++;
        }
        preBuildVersionSuffix += counter;
    }  
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
        CleanDirectory(tempDir, fsi => !fsi.Path.FullPath.EndsWith(".gitignore"));
        CreateDirectory(tempDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore("./FPLedit.sln");
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        MSBuild("./FPLedit.sln", settings => {
            settings.SetConfiguration(configuration);
            settings.Restore = true;
            settings.Properties.Add("ForceConfigurationDir", new List<string> { configuration });
            if (!string.IsNullOrEmpty(preBuildVersionSuffix))
                settings.Properties.Add("versionSuffix", new List<string> { preBuildVersionSuffix });
        });
    });

Task("PrepareArtifacts")
    .IsDependentOn("Build")
    .Does(() => {
    Console.WriteLine(buildDir);
        RequireFile(buildDir + File("Eto.DsBindComboBoxCell.Gtk.dll"));
        RequireFile(buildDir + File("Eto.DsBindComboBoxCell.Wpf.dll"));
    
        DeleteFiles(buildDir + File("*.pdb"));
        DeleteFiles(buildDir + File("*.deps.json"));
        
        var libDir = buildDir + Directory("lib");
        CreateDirectory(libDir);
        MoveFiles(buildDir + File("*.dll"), libDir);
        MoveFiles(libDir + File("FPLedit.*"), buildDir);
    });
    
Task("BuildLicenseReadme")
    .IsDependentOn("PrepareArtifacts")
    .Does(() => {
        var version = GetProductVersion(Context, buildDir + File("FPLedit.exe"));
        var text = GetLicenseText(Context, scriptsDir + File("Info.txt"), version);
        FileWriteText(buildDir + File("README_LICENSE.txt"), text);
    });
    
Task("BuildDocumentation")
    .IsDependentOn("BuildLicenseReadme")
    .Does(() => {
        if (docInPath != null && FileExists(docInPath))
        {
            var docOutPath = buildDir + File("doku.html");
            if (FileExists(docOutPath))
                DeleteFile(docOutPath);
            CopyFile(docInPath, docOutPath);
            doc_generated = true;
        }
    });
    
Task("PackRelease")
    .IsDependentOn("BuildDocumentation")
    .Does(() => {
        var version = GetProductVersion(Context, buildDir + File("FPLedit.exe"));
        var nodoc_suffix = ignoreNoDoc ? "" : (doc_generated ? "" : "-nodoc");       
        var file = Directory("./bin") + File($"fpledit-{version}{nodoc_suffix}.zip");
        
        if (FileExists(file))
            throw new Exception("Zip file already exists! " + file);
        Zip(buildDir, file);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("PackRelease")
	.Does(() => {
	    Warning("##############################################################");
	    
	    if (!doc_generated)
	        Warning("No documentation built!");

        Warning("##############################################################");
	});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////

public void RequireFile(string filename)
{
    if (!FileExists(filename))
        throw new Exception("File " + filename + " is required");
}
