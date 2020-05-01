#addin "Cake.FileHelpers&version=3.2.1"
#load "build_scripts/license.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var docInPath = EnvironmentVariable("FPLEDIT_DOK");
var ignoreNoDoc = Argument<string>("ignore_no_doc", null) != null;
var preBuildVersionSuffix = Argument("version_suffix", "");

var incrementVersion = false;

if (Argument<string>("auto-beta", null) != null) {
    ignoreNoDoc = true;
    incrementVersion = true;
    preBuildVersionSuffix = "beta";
}

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./bin") + Directory(configuration);
var buildDocDir = Directory("./bin") + Directory("api-doc");
var buildLibDir = buildDir + Directory("lib");
var sourceDir = Directory(".");
var scriptsDir = Directory("./build_scripts");

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
        CleanDirectory(buildDocDir);
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
        
        CreateDirectory(buildLibDir);
        MoveFiles(buildDir + File("*.dll"), buildLibDir);
        MoveFiles(buildLibDir + File("FPLedit.*"), buildDir);
    });
    
Task("BuildLicenseReadme")
    .IsDependentOn("PrepareArtifacts")
    .Does(() => {
        var version = GetProductVersion(Context, buildDir + File("FPLedit.exe"));
        var text = GetLicenseText(Context, 
            scriptsDir + Directory("info") + File("Info.txt"), 
            scriptsDir + Directory("info") + File("3rd-party.txt"), 
            version);
        FileWriteText(buildDir + File("README_LICENSE.txt"), text);
    });
    
Task("BundleThirdParty")
    .IsDependentOn("BuildLicenseReadme")
    .Does(() => {
        var licenseDir = buildLibDir + Directory("licenses");
        CleanDirectory(licenseDir);
        CopyFiles(scriptsDir + Directory("info") + File("3rd-party.txt"), licenseDir);
        CopyFiles(scriptsDir + Directory("info") + Directory("3rd-party") + File("*.txt"), licenseDir);
    });
    
Task("PackRelease")
    .IsDependentOn("BundleThirdParty")
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
	    
	    if (docInPath == null || docInPath == "")
	        Warning("No user documentation built!");

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
