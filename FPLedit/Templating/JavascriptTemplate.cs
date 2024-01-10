using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Esprima.Ast;
using FPLedit.Shared.Templating;
using FPLedit.Shared;
using Jint;
using Jint.Runtime.Interop;

namespace FPLedit.Templating;

// Based on: https://www.codeproject.com/Articles/15728/Write-your-own-Code-Generator-or-Template-Engine-i
internal sealed class JavascriptTemplate : ITemplate
{
    public string? TemplateType { get; private set; }
    public string TemplateName { get; private set; } = null!;
    public string Identifier { get; }
    public string TemplateSource { get; }
    public string CompiledCode { get; }

    private readonly IReducedPluginInterface pluginInterface;
    private readonly string nl = Environment.NewLine;

    private const int CURRENT_VERSION = 2;

    private Script? compiledScriptAst;

    public JavascriptTemplate(string code, string identifier, IReducedPluginInterface pluginInterface)
    {
        TemplateSource = code;
        Identifier = identifier;
        this.pluginInterface = pluginInterface;
        TemplateBuiltins.Logger = pluginInterface.Logger;

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
        switch (text[0])
        {
            case '=' when text.Length > 1:
                sb.Append("__builder += ");
                sb.Append(text[1..]);
                sb.Append(';');
                sb.Append(nl);
                break;
            case '@' when text.Length > 1:
            {
                var atRule = text[1..].Trim();
                const string tmplDef = "fpledit_template";
                if (atRule.IndexOf(tmplDef) == 0 && atRule.Length > tmplDef.Length)
                    TemplateDefinition(atRule[tmplDef.Length..].TrimStart());
                else
                    throw new FormatException("Invalid @ rule found in template!");
                break;
            }
            default:
                sb.Append(text);
                sb.Append(nl);
                break;
        }
    }
        
    private void TemplateDefinition(ReadOnlySpan<char> argsString)
    {
        if (TemplateType != null)
            throw new Exception(T._("Nur eine fpledit_template-Direktive ist pro Vorlage erlaubt!"));
        var tparams = new ArgsParser(argsString, "name", "type", "version");
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
        var idx = span[startIndex..].IndexOf(search, StringComparison.Ordinal);
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
        var allowedTypes = pluginInterface.GetRegistered<ITemplateWhitelistEntry>()
            .Where(w => w.TemplateType == TemplateType || w.TemplateType == null!)
            .SelectMany(w => w.WhitelistTypes)
            .ToList();
        var allowedExtensionsTypes = new List<Type>();
        // Globally whitelisted: From FPLedit.Shared, marked with TemplateSafeAttribute.
        foreach (var type in typeof(Timetable).Assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<TemplateSafeAttribute>(true);
            if (attr == null) continue;
            allowedTypes.Add(type);
            if (attr.AllowExtensionMethods) allowedExtensionsTypes.Add(type);
        }

        var engine = new Engine(opt =>
        {
            opt.DisableStringCompilation();

            opt.AddExtensionMethods(typeof(Enumerable)); // Allow LINQ extension methods.
            opt.AddExtensionMethods(allowedExtensionsTypes.ToArray());
        });
        foreach (var type in allowedTypes) // Register all allowed types.
            engine.SetValue(type.Name, TypeReference.CreateTypeReference(engine, type));

        // Add debugging methods.
        engine
            .SetValue("arrdep_kvp_key", TemplateBuiltins.GetKvpKey<Station, ArrDep>)
            .SetValue("debug", TemplateBuiltins.Debug)
            .SetValue("debug_print", TemplateBuiltins.DebugPrint);

        TemplateDebugger.GetInstance().SetContext(this); // Move "Debugger" context to current template.

        // Parse the template to an Esprima AST.
        compiledScriptAst ??= Engine.PrepareScript(CompiledCode, Identifier);

        var html = engine
            .SetValue("tt", tt)
            .SetValue("__builder", "") // Create output variable
            .Execute(compiledScriptAst)
            .GetValue("__builder")
            .AsString();

        return html;
    }

    private static class TemplateBuiltins
    {
        public static ILog? Logger;
        public static void Debug(object? o) => Logger?.Info($"{o?.GetType().FullName ?? "null"}: {o ?? "null"}");
        public static void DebugPrint(object? o) => Logger?.Info($"{o}");
        public static T1 GetKvpKey<T1, T2>(KeyValuePair<T1, T2> o) => o.Key;
    }
}