namespace FPLedit.Shared.Templating
{
    /// <summary>
    /// Base interface for a central template registry.
    /// </summary>
    public interface ITemplateManager
    {
        /// <summary>
        /// Retrieve all available templates of the given template type.
        /// </summary>
        ITemplate[] GetTemplates(string type);
    }
}
