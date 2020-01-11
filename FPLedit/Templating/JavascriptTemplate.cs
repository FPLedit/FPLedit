using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using FPLedit.Shared.Templating;
using FPLedit.Shared;
using Jint;

namespace FPLedit.Templating
{
    // Based on: https://www.codeproject.com/Articles/15728/Write-your-own-Code-Generator-or-Template-Engine-i
    internal class JavascriptTemplate : ITemplate
    {
        public string TemplateType { get; private set; }
        public string TemplateName { get; private set; }
        public string Identifier => throw new NotSupportedException();
        public string TemplateSource { get; }
        public string CompiledCode { get; }
        
        private readonly IInfo info;
        private readonly string nl = Environment.NewLine;

        public JavascriptTemplate(string code, IInfo info)
        {
            TemplateSource = code;
            this.info = info;

            CompiledCode =  ParseTemplate();
        }

        #region Parser
        private string ParseTemplate()
        {
            var code = TemplateSource;
            const RegexOptions ro = RegexOptions.Singleline | RegexOptions.IgnoreCase;
            const RegexOptions rom = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            // Template definition tag
            code = Regex.Replace(code, @"<#@\s*fpledit-template(.*?)#>", TemplateDefinition, rom);
            code = code.Trim('\r', '\n', ' ', '\t');

            code = ProcessTextBlocks(code);
            code = Regex.Replace(code, @"<#=(.*?)#>", TransformCalls, ro);
            code = Regex.Replace(code, @"<##>", "", ro);
            code = Regex.Replace(code, @"<#[^=|@](.*?)#>", TransformCodeTag, ro);

            return @"let __builder = '';" + nl + code;
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
            TemplateName = "JS: " + tparams["name"];
            return "";
        }

        private string TransformCalls(Match m) => "__builder +=" + m.Groups[1].ToString().Trim() + ";" + nl;
        private string TransformCodeTag(Match m) => m.Groups[1].ToString().Trim() + nl;

        private string ProcessTextBlocks(string code) // leaves code blocks untouched
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;
            
            const string startTag = "<#";
            const string endTag = "#>";
            
            var scriptBuilder = new StringBuilder();

            var nextBlockSearchIdx = 0;
            var blockStartIdx = code.IndexOf(startTag, nextBlockSearchIdx, StringComparison.Ordinal);

            while (blockStartIdx >= 0)
            {
                // Output plaintext until the start of this code block.
                scriptBuilder.Append(BuildMultilineAppend(code.Substring(nextBlockSearchIdx, blockStartIdx - nextBlockSearchIdx)));

                // Find corresponding (=next) end tag
                var blockEndIdx = code.IndexOf(endTag, blockStartIdx, StringComparison.Ordinal);
                if (blockEndIdx < 0) // This code block does not end. Throw.
                    throw new Exception("Error parsing template script: No closing code block tag found.");

                scriptBuilder.Append(code.Substring(blockStartIdx, blockEndIdx - blockStartIdx + startTag.Length)); // Add this code block to output.

                nextBlockSearchIdx = blockEndIdx + endTag.Length; // The next block can only start after the current one.
                blockStartIdx = code.IndexOf(startTag, nextBlockSearchIdx, StringComparison.Ordinal); // Find next code block
            }
            
            // Write out the final block of non-code text (No more code blocks found).
            scriptBuilder.Append(BuildMultilineAppend(code.Substring(nextBlockSearchIdx, code.Length - nextBlockSearchIdx)));

            return scriptBuilder.ToString();
        }

        private string BuildMultilineAppend(string text)
        {
            return string.Join(nl,text
                .Replace("\\", "\\\\") // escape backslash
                .Replace("\"", "\\\"") // escape quotes
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => "__builder += \"" + l + "\\n\";"))
                + nl;
        }
        #endregion

        public string GenerateResult(Timetable tt)
        {
            // Allowed types whitlisted by extensions (for this specific template type or generic (e-.g. type == null)).
            var extensionAllowedTypes = info.GetRegistered<ITemplateWhitelist>()
                .Where(w => w.TemplateType == TemplateType || w.TemplateType == null)
                .Select(w => w.GetWhitelistType());
            // Globally whitelisted: From FPLedit.Shared, marked with TemplateSafeAttribute.
            var allowedTypes = typeof(Timetable).Assembly.GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(TemplateSafeAttribute), true).Length > 0)
                .Concat(extensionAllowedTypes)
                .Concat(new[] { typeof(TimeSpan), }); // Also whitelist type used for time entries

            var engine = new Engine();
            foreach (var type in allowedTypes) // Register all allowed types
                engine.SetValue(type.Name, type);

            //TODO: Better way to show code when there are errors...
            return engine
                .SetValue("tt", tt)
                .SetValue("debug", new Action<object>((o) => info.Logger.Info($"{o.GetType().FullName}: {o}")))
                .SetValue("debug_print", new Action<object>((o) => info.Logger.Info($"{o}")))
                .Execute(CompiledCode)
                .GetValue("__builder")
                .ToString();
        }
    }
}