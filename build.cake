#addin "Cake.FileHelpers"
#load "build_scripts/license.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var docInPath = Argument("doc_path", "default value") ?? EnvironmentVariable("FPLEDIT_DOK");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./bin") + Directory(configuration);
var sourceDir = Directory(".");
var scriptsDir = Directory("./build_scripts");

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
        var file = Directory("./bin") + File($"fpledit-{version}{(doc_generated ? "" : "-nodoc")}.zip");
        if (FileExists(file))
            throw new Exception("Zip file already exists! " + file);
        Zip(buildDir, file);
    });
    
/*Task("PrepareSourceArtifacts")
    .IsDependentOn("Build")
    .Does(() => {
    Console.WriteLine(buildDir);
        CreateDirectory(sourceTempDir);
        CopyFilesRecursivelyWithExclude(Directory("."), sourceTempDir);
    });
    
Task("BuildSourceLicenseReadme")
    .IsDependentOn("PrepareSourceArtifacts")
    .Does(() => {
        var version = GetProductVersion(Context, buildDir + File("FPLedit.exe"));
        var text = GetLicenseText(Context, scriptsDir + File("Info.txt"), version);
        FileWriteText(sourceTempDir + File("README_LICENSE.txt"), text);
    });

    
Task("PackSource")
    .IsDependentOn("BuildSourceLicenseReadme")
    .Does(() => {
        var version = GetProductVersion(Context, buildDir + File("FPLedit.exe"));        
        var file = Directory("./bin") + File($"fpledit-{version}-src.zip");
        if (FileExists(file))
            throw new Exception("Zip file already exists! " + file);
        Zip(sourceTempDir, file);
    }).Finally(() => {
        CleanDirectory(sourceTempDir);
        DeleteDirectory(sourceTempDir);
    });*/

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

/*public static void CopyFilesRecursivelyWithExclude(string sourcePath, string targetPath)
{
    var source = new DirectoryInfo(sourcePath);
    var target = new DirectoryInfo(targetPath);
    var dir_exceptions = new[] { ".git", ".vs", "bin", "obj", ".idea", "build_tmp"};
    var ext_exceptions = new[] { ".user", ".gitignore", ".gitattributes", "increment" };

    int copied = 0;
    foreach (DirectoryInfo dir in source.GetDirectories())
    {
        if (dir_exceptions.Contains(dir.Name))
            continue;
        CopyFilesRecursivelyWithExclude(dir.FullName, target.CreateSubdirectory(dir.Name).FullName);
        copied++;
    }
    foreach (FileInfo file in source.GetFiles())
    {
        if (ext_exceptions.Any(e => file.Name.EndsWith(e)))
            continue;
        file.CopyTo(System.IO.Path.Combine(target.FullName, file.Name));
        copied++;
    }
    if (copied == 0) // Leere Ordner nicht mitkopieren
        target.Delete();
}*/
