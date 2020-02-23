using System.Linq;
using FPLedit.Shared.Templating;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class DefaultTemplateChooser : ITemplateChooser
    {
        private readonly string elemName, attrName, defaultTemplate;
        
        public ITemplate[] AvailableTemplates { get; }

        public DefaultTemplateChooser(IReducedPluginInterface pluginInterface, string type, string elemName, string attrName, string defaultTemplate)
        {
            this.elemName = elemName;
            this.attrName = attrName;
            this.defaultTemplate = defaultTemplate;
            
            AvailableTemplates = pluginInterface.TemplateManager.GetTemplates(type);
        }
        
        public ITemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == elemName);

            var name = attrsEn?.GetAttribute<string>(attrName) ?? "";

            return GetTemplateByName(name) ??
                   GetTemplateByName(defaultTemplate);
        }

        private ITemplate GetTemplateByName(string name)
            => AvailableTemplates.FirstOrDefault(t => t.Identifier == name);
    }
}
