using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    internal class TemplateManager : ITemplateManager
    {
        private RegisterStore store;
        private IEnumerable<TemplateHost> templates;

        public TemplateManager(RegisterStore store)
        {
            this.store = store;
        }

        public void LoadTemplates()
        {
            var instances = store.GetRegistered<ITemplateProxy>();
            templates = instances.Select(t => new TemplateHost(t.GetTemplateCode())).ToList();
        }

        public ITemplate[] GetTemplates(string type)
        {
            return templates.Where(t => t.TemplateType == type).ToArray();
        }
    }
}
