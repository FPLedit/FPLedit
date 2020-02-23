namespace FPLedit.Shared.Templating
{
    public interface ITemplateProvider
    {
        string TemplateIdentifier { get; }

        string GetTemplateCode();
    }
}
