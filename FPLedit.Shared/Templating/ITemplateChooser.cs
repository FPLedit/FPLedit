namespace FPLedit.Shared.Templating
{
    public interface ITemplateChooser
    {
        ITemplate GetTemplate(Timetable tt);

        ITemplate[] AvailableTemplates { get; }
    }
}