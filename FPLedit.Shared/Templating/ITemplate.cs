namespace FPLedit.Shared.Templating
{
    public interface ITemplate
    {
        string TemplateType { get; }

        string TemplateName { get; }

        string GenerateResult(Timetable tt);
    }
}
