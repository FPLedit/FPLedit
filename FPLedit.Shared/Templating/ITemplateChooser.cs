namespace FPLedit.Shared.Templating;

/// <summary>
/// Building block for the default template system, used in <see cref="DefaultImplementations.DefaultTemplateExport"/>,
/// providing the currently selected template.
/// </summary>
/// <remarks>See <see cref="DefaultImplementations.DefaultTemplateChooser"/> for a default implementation.</remarks>
public interface ITemplateChooser
{
    /// <summary>
    /// Get the currently selected template, based on the current timetable.
    /// </summary>
    ITemplate GetTemplate(Timetable tt);

    /// <summary>
    /// List all available tyemplates.
    /// </summary>
    ITemplate[] AvailableTemplates { get; }
}