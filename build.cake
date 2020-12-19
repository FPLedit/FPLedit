#addin "Cake.FileHelpers&version=3.2.1"
#load "build_scripts/license.cake"
using System.Text.RegularExpressions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var platform = Argument("tfm", "net5.0");
var runtimes = Argument("rid", "linux-x64");

var docInPath = EnvironmentVariable("FPLEDIT_DOK");
var hasDocInPath = !(docInPath == null || docInPath == "");
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
var buildDir = Directory("./bin") + Directory(configuration) + Directory(platform);
var buildDocDir = Directory("./bin") + Directory("api-doc");
var sourceDir = Directory(".");
var scriptsDir = Directory("./build_scripts");

if (incrementVersion && string.IsNullOrEmpty(preBuildVersionSuffix))
    throw new InvalidOperationException("No version suffix specified, but incrementVersion!");
if (incrementVersion) 
{
    var currentVersion = XmlPeek(scriptsDir + File("VersionInfo.targets"), "/Project/PropertyGroup/VersionPrefix");
    
    var fn = $"fpledit-{currentVersion}-{preBuildVersionSuffix}";
    
    if (incrementVersion) {
        var files = GetFiles(Directory("./bin") + File($"{fn}*.zip")).Select(fp => fp.GetFilename().ToString());
        var regex = new Regex($@"^{fn}(\d*).*\.zip$");
        
        var counter = files
            .Select(fn => regex.Match(fn))
            .Where(match => match.Success && match.Groups.Count == 2)
            .DefaultIfEmpty()
            .Max(match => int.Parse(match?.Groups[1]?.Value ?? "0"));
        
        preBuildVersionSuffix += (counter + 1);
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
        ForAllRuntimes((rt, dd) => CleanDirectory(dd));
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("./FPLedit.sln");
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        var msbuildSettings = new DotNetCoreMSBuildSettings();
        if (!string.IsNullOrEmpty(preBuildVersionSuffix))
            msbuildSettings.Properties.Add("versionSuffix", new List<string> { preBuildVersionSuffix });
        DotNetCoreBuild("./FPLedit.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = false,
            MSBuildSettings = msbuildSettings,
        });
    });
    
Task("PackNet5")
    .IsDependentOn("Build")
    .Does(() =>
    {
        ForAllRuntimes( (runtime, distDir) => {
            var msbuildSettings = new DotNetCoreMSBuildSettings();
            if (!string.IsNullOrEmpty(preBuildVersionSuffix))
                msbuildSettings.Properties.Add("versionSuffix", new List<string> { preBuildVersionSuffix });
            msbuildSettings.Properties.Add("BaseOutputAppPath", new List<string> { System.IO.Path.GetFullPath(distDir.Path.FullPath + "/") });
            DotNetCorePublish("./FPLedit.sln", new DotNetCorePublishSettings {
                Configuration = configuration,
                Runtime = runtime,
                SelfContained = false,
                OutputDirectory = distDir,
                MSBuildSettings = msbuildSettings,
            });
        });
    });
    
Task("BuildUserDocumentation")
    .IsDependentOn("PackNet5")
    .Does(() =>
    {
        if (hasDocInPath) {
            MSBuild("./build_scripts/GenerateUserDocumentation.proj", settings => {
                settings.Properties.Add("OutputPath", new List<string> { System.IO.Path.GetFullPath(buildDir.Path.FullPath) });
            });
            ForAllRuntimes( (runtime, distDir) => {
                CopyFiles(buildDir + File("*.pdf"), distDir);
            });
            //TODO: Generate in temp file and copy to all.
        }
    });

Task("PrepareArtifacts")
    .IsDependentOn("BuildUserDocumentation")
    .Does(() => {
    });
    
Task("BuildLicenseReadme")
    .IsDependentOn("PrepareArtifacts")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var version = GetProductVersion(Context, distDir + File("FPLedit.dll"));
            var text = GetLicenseText(Context, 
                scriptsDir + Directory("info") + File("Info.txt"), 
                scriptsDir + Directory("info") + File("3rd-party.txt"), 
                version);
            FileWriteText(distDir + File("README_LICENSE.txt"), text);
        });
    });
    
Task("BundleThirdParty")
    .IsDependentOn("BuildLicenseReadme")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var licenseDir = distDir + Directory("licenses");
            CleanDirectory(licenseDir);
            CopyFiles(scriptsDir + Directory("info") + File("3rd-party.txt"), licenseDir);
            CopyFiles(scriptsDir + Directory("info") + Directory("3rd-party") + File("*.txt"), licenseDir);
        });
    });
    
Task("PackRelease")
    .IsDependentOn("BundleThirdParty")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var version = GetProductVersion(Context, distDir + File("FPLedit.dll"));
            var nodoc_suffix = ignoreNoDoc ? "" : (!hasDocInPath ? "-nodoc" : "");       
            var file = Directory("./bin") + File($"fpledit-{version}-{runtime}{nodoc_suffix}.zip");
            
            if (FileExists(file))
                throw new Exception("Zip file already exists! " + file);
                
            if (!runtime.StartsWith("osx")) {
                Zip(distDir, file);
            } else {
                var macBundle = distDir + Directory("FPLedit.app");
                var macTarget = macBundle + Directory("Contents") + Directory("MacOS");
                
                var filesToZip = new List<string>();
                filesToZip.AddRange(GetFiles(macBundle + File("**/*")).Select(f => f.FullPath));
                filesToZip.Add(distDir + File("README_LICENSE.txt"));
                if (hasDocInPath)
                    filesToZip.Add(distDir + File("Dokumentation.pdf"));
                filesToZip.AddRange(GetFiles(distDir + Directory("licenses") + File("*")).Select(f => f.FullPath));
                
                Zip(distDir, file, filesToZip);
            }
        });
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

public void ForAllRuntimes(Action<string, ConvertableDirectoryPath> func) {
    var rts = runtimes.Split(',');
    foreach (var rt in rts) {
        var distDir = Directory("./bin") + Directory("dist") + Directory(rt);
        func(rt, distDir);
    }
}
