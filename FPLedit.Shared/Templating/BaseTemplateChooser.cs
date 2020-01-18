using System.Linq;

namespace FPLedit.Shared.Templating
{
    public abstract class BaseTemplateChooser
    {
        protected abstract string DefaultTemplate { get; }

        protected abstract string ElemName { get; }

        protected abstract string AttrName { get; }

        protected BaseTemplateChooser(string type, IPluginInterface pluginInterface)
        {
            AvailableTemplates = pluginInterface.TemplateManager.GetTemplates(type);
        }

        public ITemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == ElemName);

            var name = attrsEn?.GetAttribute<string>(AttrName) ?? "";

            return GetTemplateByName(name) ??
                GetTemplateByName(DefaultTemplate);
        }

        private ITemplate GetTemplateByName(string name)
            => AvailableTemplates.FirstOrDefault(t => t.Identifier == name);

        public ITemplate[] AvailableTemplates { get; private set; }
    }
}
