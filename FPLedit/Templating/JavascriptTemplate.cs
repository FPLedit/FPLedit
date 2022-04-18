using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Esprima;
using FPLedit.Shared.Templating;
using FPLedit.Shared;
using Jint;

namespace FPLedit.Templating
{
    // Based on: https://www.codeproject.com/Articles/15728/Write-your-own-Code-Generator-or-Template-Engine-i
    internal sealed class JavascriptTemplate : ITemplate
    {
        public string TemplateType { get; private set; }
        public string TemplateName { get; private set; }
        public string Identifier { get; }
        public string TemplateSource { get; }
        public string CompiledCode { get; }

        private readonly IReducedPluginInterface pluginInterface;
        private readonly string nl = Environment.NewLine;

        private const int CURRENT_VERSION = 2;

        public JavascriptTemplate(string code, string identifier, IReducedPluginInterface pluginInterface)
        {
            TemplateSource = code;
            Identifier = identifier;
            this.pluginInterface = pluginInterface;

            CompiledCode = ParseTemplate(TemplateSource.AsSpan());
        }

        #region Parser

        private string ParseTemplate(ReadOnlySpan<char> code)
        {
            if (code.IsWhiteSpace())
                return string.Empty;
 
            const string startTag = "<#";
            const string endTag = "#>";

            var scriptBuilder = new StringBuilder(code.Length + 1024);

            var nextBlockSearchIdx = 0;
            var blockStartIdx = IndexOfWithStart(code, startTag, nextBlockSearchIdx);

            while (blockStartIdx >= 0)
            {
                // Output plaintext until the start of this code block.
                BuildMultilineAppend(scriptBuilder, code.Slice(nextBlockSearchIdx, blockStartIdx - nextBlockSearchIdx));

                // Find corresponding (=next) end tag
                var blockEndIdx = IndexOfWithStart(code, endTag, blockStartIdx);
                if (blockEndIdx < 0) // This code block does not end. Throw.
                    throw new FormatException("Error parsing template script: No closing code block tag found.");

                AppendCodeTag(scriptBuilder, code.Slice(blockStartIdx + startTag.Length, blockEndIdx - blockStartIdx - endTag.Length)); // Add this code block to output.

                nextBlockSearchIdx = blockEndIdx + endTag.Length; // The next block can only start after the current one.
                blockStartIdx = IndexOfWithStart(code, startTag, nextBlockSearchIdx); // Find next code block
            }

            // Write out the final block of non-code text (No more code blocks found).
            BuildMultilineAppend(scriptBuilder, code.Slice(nextBlockSearchIdx, code.Length - nextBlockSearchIdx));

            return scriptBuilder.ToString();
        }

        private void AppendCodeTag(StringBuilder sb, ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return;
            if (text[0] == '=' && text.Length > 1)
            {
                sb.Append("__builder += ");
                sb.Append(text.Slice(1));
                sb.Append(';');
                sb.Append(nl);
            }
            else if (text[0] == '@' && text.Length > 1)
            {
                var atRule = text.Slice(1).Trim();
                const string tmplDef = "fpledit_template";
                if (atRule.IndexOf(tmplDef) != -1 && atRule.Length > tmplDef.Length)
                    TemplateDefinition(atRule.Slice(tmplDef.Length).TrimStart());
                else
                    throw new FormatException("Invalid @ rule found in template!");
            }
            else
            {
                sb.Append(text);
                sb.Append(nl);
            }
        }
        
        private void TemplateDefinition(ReadOnlySpan<char> argsString)
        {
            if (TemplateType != null)
                throw new Exception(T._("Nur eine fpledit_template-Direktive ist pro Vorlage erlaubt!"));
            var tparams = new ArgsParser(argsString.ToString(), "name", "type", "version");
            foreach (var (name, val) in tparams)
            {
                switch (name)
                {
                    case "version":
                        if (val != CURRENT_VERSION.ToString())
                            throw new Exception(T._("Template-version mismatch! (Current: {0} vs {1})", CURRENT_VERSION, val));
                        break;
                    case "type":
                        TemplateType = val;
                        break;
                    case "name":
                        TemplateName = val;
                        break;
                }
            }
            if (!tparams.FoundAll())
                throw new Exception(T._("Fehlende Angabe type, version oder name in der fpledit_template-Direktive!"));
        }

        private int IndexOfWithStart(ReadOnlySpan<char> span, ReadOnlySpan<char> search, int startIndex)
        {
            var idx = span.Slice(startIndex).IndexOf(search, StringComparison.Ordinal);
            return idx != -1 ? idx + startIndex : -1;
        }

        private void BuildMultilineAppend(StringBuilder sb, ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return;
            var splitter = new SpanSplitEnumerator<char>(text, "\r\n".AsSpan());
            var hadLast = false;
            foreach (var range in splitter)
            {
                if (range.End.Value - range.Start.Value < 0)
                    continue;

                var line = text[range];
                if (line.IsEmpty)
                    continue;

                if (hadLast)
                {
                    sb.Append("\\n\";");
                    sb.Append(nl);
                }
                
                sb.Append("__builder += \"");
                EscapeBackslashAndQuotes(sb, line);
                hadLast = true;
            }

            if (hadLast)
            {
                sb.Append("\";");
                sb.Append(nl);
            }
        }

        private void EscapeBackslashAndQuotes(StringBuilder sb, ReadOnlySpan<char> line)
        {
            var bsSplitter = new SpanSplitEnumerator<char>(line, "\\".AsSpan());
            bool hadBs = false;
            foreach (var bsPart in bsSplitter)
            {
                if (bsPart.End.Value - bsPart.Start.Value < 0)
                    continue;
                
                if (hadBs)
                    sb.Append("\\\\");

                var bsLine = line[bsPart];
                if (bsLine.IsEmpty)
                    continue;
                var quotSplitter = new SpanSplitEnumerator<char>(bsLine, "\"".AsSpan());
                bool hadQuot = false;
                foreach (var quotPart in quotSplitter)
                {
                    if (quotPart.End.Value - quotPart.Start.Value < 0)
                        continue;
                    if (hadQuot)
                        sb.Append("\\\"");
                    sb.Append(bsLine[quotPart]);
                    hadQuot = true;
                }

                hadBs = true;
            }
        }

        #endregion

        public string GenerateResult(Timetable tt)
        {
            // Allowed types whitlisted by extensions (for this specific template type or generic (e.g. type == null)).
            var extensionAllowedTypes = pluginInterface.GetRegistered<ITemplateWhitelistEntry>()
                .Where(w => w.TemplateType == TemplateType || w.TemplateType == null)
                .Select(w => w.GetWhitelistType());
            // Globally whitelisted: From FPLedit.Shared, marked with TemplateSafeAttribute.
            var allowedTypes = typeof(Timetable).Assembly.GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(TemplateSafeAttribute), true).Length > 0)
                .Concat(extensionAllowedTypes)
                .Concat(new[] { typeof(Enumerable) }); // Also whitelist type used for LINQ.

            var engine = new Engine();
            foreach (var type in allowedTypes) // Register all allowed types
                engine.SetValue(type.Name, type);

            TemplateDebugger.GetInstance().SetContext(this); // Move "Debugger" context to current template.

            const string polyFillsPath = "Templating.TemplatePolyfills.js";
            var polyfillsParserOptions = new ParserOptions(polyFillsPath) { Tolerant = false };
            var templateCodeParserOptions = new ParserOptions(Identifier);
            
            return engine
                .SetValue("tt", tt)
                .SetValue("debug", new Action<object>((o) => pluginInterface.Logger.Info($"{o?.GetType()?.FullName ?? "null"}: {o ?? "null"}")))
                .SetValue("debug_print", new Action<object>((o) => pluginInterface.Logger.Info($"{o}")))
                .SetValue("clr_typename", new Func<object,string>(o => o.GetType().Name))
                .SetValue("clr_typefullname", new Func<object,string>(o => o.GetType().FullName))
                .SetValue("clr_toArray", new Func<IEnumerable<object>,object[]>(o => o.ToArray()))
                .SetValue("safe_html", (Func<string,string>)TemplateOutput.SafeHtml)
                .SetValue("safe_css_str", (Func<string,string>)TemplateOutput.SafeCssStr)
                .SetValue("safe_css_block", (Func<string,string>)TemplateOutput.SafeCssBlock)
                .Execute(ResourceHelper.GetStringResource(polyFillsPath), polyfillsParserOptions) // Load polyfills
                .Execute("var __builder = '';", polyfillsParserOptions) // Create output variable
                .Execute(CompiledCode, templateCodeParserOptions)
                .GetValue("__builder")
                .ToString();
        }
    }
}