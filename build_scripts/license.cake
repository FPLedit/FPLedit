public static string GetLicenseText(ICakeContext context, string infoPath, string licensePath, string thirdPartyInfoPath, string version)
{   
    var template_header = @"FPLedit Version {0}

(c) 2015-{1} Manuel Huber
https://fahrplan.manuelhu.de/

Informationen zu eingebundenen Bibliotheken von anderen Entwicklern finden Sie am Ende dieser Datei oder im Ordner lib/licenses.

";

    var text = string.Format(template_header, version, DateTime.Now.Year);
    text += System.IO.File.ReadAllText(infoPath);
    text += System.IO.File.ReadAllText(licensePath);
    text += @"


=========================================
EINGEBUNDENE BIBLIOTHEKEN
=========================================

";

    text += System.IO.File.ReadAllText(thirdPartyInfoPath);
    return AddNewlines(text);
}


static string GetProductVersion(ICakeContext context, string appPath)
{
    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(appPath).ProductVersion;
    context.Information("Verwendete Versionsinfo: " + version + "\n");
    return version;
}

static string AddNewlines(string text, int length = 75)
{
    var ret = "";
    string[] sections = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
    foreach (var sect in sections)
    {
        string[] parts = sect.Split(' ');
        var block = parts[0];
        int z = parts[0].Length;
        for (int i = 1; i < parts.Length; i++)
        {
            if ((z + 1 + parts[i].Length) > length)
            {
                block += Environment.NewLine + parts[i];
                z = parts[i].Length;
            }
            else
            {
                block += " " + parts[i];
                z += 1 + parts[i].Length;
            }
        }
        ret += block + Environment.NewLine;
    }

    return ret;
}
