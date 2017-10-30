using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Templating
{
    internal class TemplateManager : ITemplateManager
    {
        private RegisterStore store;
        private ILog logger;
        private IEnumerable<TemplateHost> templates;

        public TemplateManager(RegisterStore store, ILog logger)
        {
            this.store = store;
            this.logger = logger;
        }

        public void LoadTemplates()
        {
            var instances = store.GetRegistered<ITemplateProxy>();
            templates = instances.Select(t => new TemplateHost(t.GetTemplateCode(), logger)).ToList();
        }

        public ITemplate[] GetTemplates(string type)
        {
            return templates.Where(t => t.TemplateType == type).ToArray();
        }
    }
}
