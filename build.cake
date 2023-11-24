#load "build_scripts/license.cake"
using System.Text.RegularExpressions;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var platform = Argument("tfm", "net6.0");
var runtimes = Argument("rid", "linux-x64");

var copyDocPdf = EnvironmentVariable("FPLEDIT_DOK_PDF");

var ignoreNoDoc = Argument<string>("ignore_no_doc", null) != null;
var preBuildVersionSuffix = Argument("version_suffix", "");

var incrementVersion = false;
var isNonFinalVersion = false;
string gitRevision = null;

// Automatic beta mode (local build)
if (HasArgument("auto-beta")) {
    ignoreNoDoc = true;
    incrementVersion = true;
    isNonFinalVersion = true;
    preBuildVersionSuffix = "beta";
}

// Git beta mode (CI build)
if (EnvironmentVariable<string>("FPLEDIT_GIT", null) != null) {
    ignoreNoDoc = true;
    isNonFinalVersion = EnvironmentVariable<string>("FPLEDIT_GIT_BETA", null) != null;
    gitRevision = EnvironmentVariable<string>("FPLEDIT_GIT", "");
    var shortRevision = gitRevision.Length > 7 ? gitRevision.Substring(0, 7) : gitRevision;
    preBuildVersionSuffix = isNonFinalVersion ? $"git-{shortRevision}-{DateTime.Now:yyyyMMdd}" : "";
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
        var files = GetFiles($"./bin/{fn}*.zip").Select(fp => fp.GetFilename().ToString());
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
        DotNetRestore("./FPLedit.sln");
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        var msbuildSettings = new DotNetMSBuildSettings();
        if (!string.IsNullOrEmpty(preBuildVersionSuffix))
            msbuildSettings.Properties.Add("versionSuffix", new List<string> { preBuildVersionSuffix });
        DotNetBuild("./FPLedit.sln", new DotNetBuildSettings
        {
            Configuration = configuration,
            NoRestore = false,
            MSBuildSettings = msbuildSettings,
        });
    });
    
Task("PackNet")
    .IsDependentOn("Build")
    .Does(() =>
    {
        ForAllRuntimes( (runtime, distDir) => {
            var msbuildSettings = new DotNetMSBuildSettings();
            msbuildSettings.Properties.Add("IsNonFinalFPLeditBuild", new List<string> { isNonFinalVersion.ToString() });
            if (!string.IsNullOrEmpty(gitRevision))
                msbuildSettings.Properties.Add("GitRevision", new List<string> { gitRevision });
            if (!string.IsNullOrEmpty(preBuildVersionSuffix))
                msbuildSettings.Properties.Add("versionSuffix", new List<string> { preBuildVersionSuffix });
            msbuildSettings.Properties.Add("BaseOutputAppPath", new List<string> { IOPath.GetFullPath(distDir.Path.FullPath + "/") });
            DotNetPublish("./FPLedit.sln", new DotNetPublishSettings {
                Configuration = configuration,
                Runtime = runtime,
                SelfContained = runtime.StartsWith("osx"),
                OutputDirectory = distDir,
                MSBuildSettings = msbuildSettings,
            });
        });
    });


var hasDocInZip = false;
Task("BuildUserDocumentation")
    .IsDependentOn("PackNet")
    .Does(() =>
    {
        if (!string.IsNullOrEmpty(EnvironmentVariable("FPLEDIT_DOK_REPO"))) {
            throw new Exception("Building doc PDF not supported any more...");
        } else if (!string.IsNullOrEmpty(copyDocPdf)) {
            ForAllRuntimes( (runtime, distDir) => {
                CopyFiles(copyDocPdf, distDir);
            });
            hasDocInZip = true;
        }
    });

Task("PrepareArtifacts")
    .IsDependentOn("BuildUserDocumentation")
    .Does(() => {
        // Cleanup satellite assemblies in languages not supported in FPLedit.
        var foldersToDelete = new[] { "es", "fr", "hu", "it", "pt-BR", "ro", "ru", "sv", "zh-Hans" };
        ForAllRuntimes( (runtime, distDir) => {
            foreach (var languageFolder in foldersToDelete) {
                var d = distDir + Directory(languageFolder);
                if (DirectoryExists(d)) {
                    DeleteDirectory(d, new DeleteDirectorySettings { Recursive = true });
                }
            }
            
            // Delete beta extensions.
            if (!isNonFinalVersion)
                DeleteFiles((string)distDir + "/FPLedit.GTFS.*");
        });
    });
    
Task("BuildLicenseReadme")
    .IsDependentOn("PrepareArtifacts")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var version = GetProductVersion(Context, distDir + File("FPLedit.dll"));
            var text = GetLicenseText(Context,
                scriptsDir + Directory("info") + File("Info.txt"),
                scriptsDir + Directory("info") + File("gpl.txt"),
                scriptsDir + Directory("info") + File("3rd-party.txt"),
                version);
            IOFile.WriteAllText((distDir + File("README_LICENSE.txt")).ToString(), text);
        });
    });
    
Task("BundleThirdParty")
    .IsDependentOn("BuildLicenseReadme")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var licenseDir = distDir + Directory("licenses");
            CleanDirectory(licenseDir);
            CopyFiles((string)scriptsDir + "/info/3rd-party.txt", licenseDir);
            CopyFiles((string)scriptsDir + "/info/3rd-party/*.txt", licenseDir);
        });
    });
    
var zipFileHashes = new List<string>();
    
Task("PackRelease")
    .IsDependentOn("BundleThirdParty")
    .Does(() => {
        ForAllRuntimes( (runtime, distDir) => {
            var version = GetProductVersion(Context, distDir + File("FPLedit.dll"));
            var nodoc_suffix = ignoreNoDoc ? "" : (!hasDocInZip ? "-nodoc" : "");
            var zip_file_name = $"fpledit-{version}-{runtime}{nodoc_suffix}.zip";
            var file = Directory("./bin") + File(zip_file_name);
            
            if (FileExists(file))
                throw new Exception("Zip file already exists! " + file);
                
            if (!runtime.StartsWith("osx")) {
                Zip(distDir, file);
            } else {
                var macBundle = distDir + Directory("FPLedit.app");
                var macTarget = macBundle + Directory("Contents") + Directory("MacOS");
                
                var filesToZip = new List<string>();
                filesToZip.AddRange(GetFiles((string)macBundle + "/**/*").Select(f => f.FullPath));
                filesToZip.Add(distDir + File("README_LICENSE.txt"));
                if (hasDocInZip)
                    filesToZip.Add(distDir + File("Dokumentation.pdf"));
                filesToZip.AddRange(GetFiles((string)distDir + "/licenses/*").Select(f => f.FullPath));
                
                Zip(distDir, file, filesToZip);
            }

            // Create hash line
            var fhc = new FileHashCalculator(new FileSystem());
            var hash = fhc.Calculate(file, HashAlgorithm.SHA256).ToHex();
            var hash_line = $"{hash}  {zip_file_name}";
            zipFileHashes.Add(hash_line);
            IOFile.AppendAllText($"./bin/fpledit-{version}-{nodoc_suffix}.sha256sums", hash_line + "\n");
        });
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("PackRelease")
	.Does(() => {
	    Information("##############################################################");
	    
	    if (!hasDocInZip)
	        Warning("No user documentation built!");
	    
	    foreach (var hash_line in zipFileHashes) {	    	
	    	Information(hash_line);
	    }

        Information("##############################################################");
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
