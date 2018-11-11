using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using FPLedit.Shared.Templating;
using FPLedit.Shared;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.IO;

namespace FPLedit.Templating
{
    // Based on: https://www.codeproject.com/Articles/15728/Write-your-own-Code-Generator-or-Template-Engine-i
    internal class Template : ITemplate
    {
        private string code;
        private List<string> usings;
        private List<string> assemblyReferences;
        private string functions;

        public string TemplateType { get; private set; }
        public string TemplateName { get; private set; }
        public string Identifier => throw new NotSupportedException();
        public string TemplateSource => code;

        private string _codeCache;

        private readonly string nl = Environment.NewLine;

        public Template(string code)
        {
            this.code = code + "<##>";
            usings = new List<string>() { "System", "System.Collections.Generic", "System.Text", "System.Linq", "FPLedit.Shared", "FPLedit.Shared.Helpers" };
            assemblyReferences = new List<string>();

            BuildCodeCache();
        }

        #region Parser

        private void BuildCodeCache()
        {
            if (_codeCache != null)
                return;
            var body = GetMethodBody();
            string usingsCode = string.Join("", usings.Distinct().Select(u => $"using {u};{nl}"));

            _codeCache = $@"{usingsCode}

[assembly:System.Security.AllowPartiallyTrustedCallers]

namespace FPLedit.Shared.Templating
{{
	public class TemplateParser
	{{
		public static string Render(Timetable tt)
		{{
			return new TemplateParser().InternalRender(tt);
		}}

		private string InternalRender(Timetable tt)
		{{
			StringBuilder builder = new StringBuilder();

			{body}

			return builder.ToString();
		}}

		{functions}
	}}
}}";
        }


        private string GetMethodBody()
        {
            string mainCode = code;
            var ro = RegexOptions.Singleline | RegexOptions.IgnoreCase;
            var rom = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            // import, assembly & define tag
            mainCode = Regex.Replace(mainCode, @"<#@\s*assembly(.*?)#>", AddAssembly, ro);
            mainCode = Regex.Replace(mainCode, @"<#@\s*import(.*?)#>", AddImport, ro);
            mainCode = Regex.Replace(mainCode, @"<#@\s*define([\S\s]*?)#>", DefineFunction, rom);
            mainCode = Regex.Replace(mainCode, @"<#@\s*fpledit-template(.*?)#>", TemplateDefinition, rom);
            mainCode = mainCode.Trim('\r', '\n', ' ', '\t');

            mainCode = ParseScript(mainCode);
            mainCode = Regex.Replace(mainCode, @"<#=(.*?)#>", RefineCalls, ro);
            mainCode = Regex.Replace(mainCode, @"<##>", "", ro);
            mainCode = Regex.Replace(mainCode, @"<#[^=|@](.*?)#>", RefineCodeTag, ro);

            return mainCode;
        }

        private string AddAssembly(Match m)
        {
            string fn = m.Groups[1].ToString().Trim();
            assemblyReferences.Add(fn);
            return "";
        }

        private string AddImport(Match m)
        {
            string ns = m.Groups[1].ToString().Trim();
            usings.Add(ns);
            return "";
        }

        private string DefineFunction(Match m)
        {
            string fun = m.Groups[1].ToString().Trim();
            functions += fun + nl;
            return "";
        }

        private string TemplateDefinition(Match m)
        {
            if (TemplateType != null)
                throw new Exception("Nur eine fpledit-template-Direktive pro Vorlage erlaubt!");
            var args = m.Groups[1].ToString().Trim();
            var tparams = new ArgsParser(args).ParsedArgs;
            if (!tparams.ContainsKey("type") || !tparams.ContainsKey("name"))
                throw new Exception("Fehlende Angabe type oder name in der fpledit-template-Direktive!");
            TemplateType = tparams["type"];
            TemplateName = tparams["name"];
            return "";
        }

        private string RefineCalls(Match m)
        {
            string c = m.Groups[1].ToString().Trim();
            return "\t\t\tbuilder.Append(" + c + ");";
        }

        private string RefineCodeTag(Match m)
        {
            string c = m.Groups[1].ToString().Trim();
            return "\t\t\t" + c + nl;
        }

        private string ParseScript(string code)
        {
            if (code == null)
                return "";
            StringBuilder builder = new StringBuilder();

            int lnLast = 0;
            int lnAt2 = 0;
            int lnAt = code.IndexOf("<#", 0);
            if (lnAt == -1)
                return code;

            while (lnAt > -1)
            {
                if (lnAt > -1)
                    // Catch the plain text write out to the Response Stream as is - fix up for quotes
                    builder.Append("builder.Append(@\"" + code.Substring(lnLast, lnAt - lnLast).Replace("\"", "\"\"") + "\");" + nl);

                // Find end tag
                lnAt2 = code.IndexOf("#>", lnAt);
                if (lnAt2 < 0)
                    break;

                builder.Append(code.Substring(lnAt, lnAt2 - lnAt + 2));

                lnLast = lnAt2 + 2;
                lnAt = code.IndexOf("<#", lnLast);
                if (lnAt < 0)
                    // Write out the final block of non-code text
                    builder.Append("builder.Append(@\"" + code.Substring(lnLast, code.Length - lnLast).Replace("\"", "\"\"") + "\");" + nl);
            }

            return builder.ToString();
        }

        #endregion

        public string GenerateResult(Timetable tt)
        {
            try
            {
                BuildCodeCache(); // Ensure that we have code to compile

                AppDomainSetup adSetup = new AppDomainSetup()
                {
                    ApplicationBase = Path.GetFullPath("sandbox" + DateTime.Now.Ticks),
                };
                AppDomain domain;

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    domain = CreateSandboxDomain(adSetup); // Create new, isolated AppDomain (currently only Windows)
                else
                    domain = AppDomain.CreateDomain("tmpl-run-domain", null, adSetup); // Mono: No sandboxing possible at the moment.

                // Start compiler in another AppDomain
                var sandbox = (TemplateSandbox)Activator.CreateInstanceFrom(domain, typeof(TemplateSandbox).Assembly.Location, typeof(TemplateSandbox).FullName).Unwrap();
                sandbox.InstallResolver(AppDomain.CurrentDomain.BaseDirectory);
                var ret = sandbox.CompileInAppDomain(tt, _codeCache, assemblyReferences.ToArray());

                AppDomain.Unload(domain);

                return ret;
            }
            catch
            {
                throw;
            }
        }

        private AppDomain CreateSandboxDomain(AppDomainSetup adSetup)
        {
            // Create permissions
            var permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.UnmanagedCode));
            permSet.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            permSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

            var fperm = new FileIOPermission(PermissionState.None);
            fperm.AllLocalFiles = FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery;
            fperm.AddPathList(FileIOPermissionAccess.AllAccess, Path.GetTempPath());
            permSet.AddPermission(fperm);

            var sharedAssembly = typeof(Timetable).Assembly.Evidence.GetHostEvidence<StrongName>();
            var clientAssembly = typeof(Template).Assembly.Evidence.GetHostEvidence<StrongName>();

            // Create new, isolated AppDomain
            return AppDomain.CreateDomain("tmpl-run-domain", null, adSetup, permSet, sharedAssembly, clientAssembly);
        }
    }
}
