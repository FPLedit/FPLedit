using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using Jint.Runtime;

namespace FPLedit.Templating;

/// <summary>
/// Exception-Handling for compiled templates.
/// </summary>
internal sealed class TemplateHost : ITemplate
{
    private readonly ITemplate? tmpl;
    private readonly ILog logger;

    public string? TemplateType => tmpl?.TemplateType;

    public string TemplateName => tmpl?.TemplateName ?? "<kein Template-Name>";

    public string Identifier { get; }

    public string? TemplateSource => tmpl?.TemplateSource;

    public bool Enabled { get; private set; }

    public TemplateHost(string content, string identifier, IReducedPluginInterface pluginInterface, bool enabled)
    {
        logger = pluginInterface.Logger;
        Identifier = identifier;
        Enabled = enabled;

        try
        {
            tmpl = new JavascriptTemplate(content, identifier, pluginInterface);

            if (tmpl?.TemplateType == null)
                logger.Warning(
                    T._("Keine valide Template-Deklaration gefunden! Das Template steht deshalb nicht zur Verfügung!"));
        }
        catch (Exception ex)
        {
            logger.Error(T._("Init-Fehler im Template {0}: {1}", Identifier, ex.Message));
        }
    }

    public string? GenerateResult(Timetable tt)
    {
        if (!Enabled || tmpl == null)
            return null;

        try
        {
            return tmpl.GenerateResult(tt);
        }
        catch (JavaScriptException ex)
        {
            var source = ex.Location.Source ?? Identifier;
            var isModule = source != Identifier;
            var loc = ex.Location.Start;
            logger.Error(T._("Fehler im {0} {1}: {2} in line {3}, column {4}",
                (isModule ? "Modul" : "Template"), source, ex.Message, loc.Line, loc.Column));
            if (!isModule)
                TemplateDebugger.GetInstance().Navigate(loc.Line, loc.Column);
        }
        catch (Esprima.ParserException ex)
        {
            var source = ex.Source ?? Identifier;
            var isModule = source != Identifier;
            logger.Error(T._("Fehler im {0} {1}: {2} in line {3}, column {4}",
                (isModule ? "Modul" : "Template"), source, ex.Message, ex.LineNumber, ex.Column));
            if (!isModule)
                TemplateDebugger.GetInstance().Navigate(ex.LineNumber, ex.Column);
        }
        catch (Exception ex)
        {
            logger.Error(T._("Fehler im Template {0}: {1}", Identifier, ex.Message));
            if (ex is not TemplateOutputException) // Filter out validation errors.
                TemplateDebugger.GetInstance().OpenDebugger();
        }

        return null;
    }
}