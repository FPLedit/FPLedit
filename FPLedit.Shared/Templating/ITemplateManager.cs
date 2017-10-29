namespace FPLedit.Shared.Templating
{
    public interface ITemplateManager
    {
        ITemplate[] GetTemplates(string type);
    }
}
