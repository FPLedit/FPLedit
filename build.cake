#addin "Cake.FileHelpers&version=3.2.1"
#load "build_scripts/license.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var docInPath = Argument("doc_path", "default value") ?? EnvironmentVariable("FPLEDIT_DOK");
var ignoreNoDoc = bool.Parse(Argument("ignore_no_doc", "false"));
var versionSuffix = Argument("version_suffix", "");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./bin") + Directory(configuration);
var sourceDir = Directory(".");
var scriptsDir = Directory("./build_scripts");
var tempDir = Directory("./build_tmp");

var sourceTempDir = Directory(System.IO.Path.GetTempPath()) + Directory("fpledit_source_build_tmp");

var doc_generated = false;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
        CleanDirectory(sourceTempDir);
        CreateDirectory(tempDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore("./FPLedit.sln");
    });
    
Task("CreateBuildFlags")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => {
        var file = tempDir + File("BuildFlags.cs");
        FileWriteText(file, $"[assembly: AssemblyVersionFlagAttribute(\"{versionSuffix}\")]");
    });

Task("Build")
    .IsDependentOn("CreateBuildFlags")
    .Does(() =>
    {
        MSBuild("./FPLedit.sln", settings => {
            settings.SetConfiguration(configuration);
            settings.Restore = true;
            settings.Properties.Add("ForceConfigurationDir", new List<string> { configuration });
        });
    });
    
Task("DeleteBuildFlags")
    .IsDependentOn("Build")
    .Does(() => {
        var file = tempDir + File("BuildFlags.cs");
        DeleteFile(file);
    });

Task("PrepareArtifacts")
    .IsDependentOn("DeleteBuildFlags")
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
        var version_suffix_suffix = versionSuffix == "" ? "" : ("-" + versionSuffix);
        var text = GetLicenseText(Context, scriptsDir + File("Info.txt"), version + version_suffix_suffix);
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
        var version_suffix_suffix = versionSuffix == "" ? "" : ("-" + versionSuffix);
        var file = Directory("./bin") + File($"fpledit-{version}{version_suffix_suffix}{nodoc_suffix}.zip");
        
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
