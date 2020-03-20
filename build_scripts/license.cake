public static string GetLicenseText(ICakeContext context, string template, string version)
{
    var info = context.FileReadText(template);
    
    var template_header = @"FPLedit Version {0}
    
    (c) 2015-{1} Manuel Huber
    https://fahrplan.manuelhu.de/
    
    ";

    var tmpl = template_header + AddNewlines(info);
    var year = DateTime.Now.Year;
    return string.Format(tmpl, version, year);
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
