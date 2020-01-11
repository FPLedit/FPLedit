namespace FPLedit.Shared.Templating
{
    public interface ITemplateProxy
    {
        string TemplateIdentifier { get; }

        string GetTemplateCode();
        
        bool Javascript { get; }
    }
}
