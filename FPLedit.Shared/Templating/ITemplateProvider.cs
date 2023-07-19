namespace FPLedit.Shared.Templating;

/// <summary>
/// Registrable provider for a single embedded template.
/// </summary>
public interface ITemplateProvider : IRegistrableComponent
{
    /// <summary>
    /// Unique identifier for the template, usually a path starting with "builtin:"
    /// </summary>
    string TemplateIdentifier { get; }

    /// <summary>
    /// Returns the template code to be compiled.
    /// </summary>
    string GetTemplateCode();
}