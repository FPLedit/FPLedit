namespace FPLedit.Shared.Templating
{
    /// <summary>
    /// Base interface for Templates, providing basic functionality to invoke templates.
    /// </summary>
    /// <remarks> No custom implementations will be used nor can be registered.</remarks>
    public interface ITemplate
    {
        string TemplateType { get; }

        string TemplateName { get; }

        string Identifier { get; }

        string TemplateSource { get; }

        string? GenerateResult(Timetable tt);
    }
}
