using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit.Templating
{
    internal class TemplateManager : ITemplateManager
    {
        private RegisterStore store;
        private ILog logger;
        private List<TemplateHost> templates;

        public TemplateManager(RegisterStore store, ILog logger)
        {
            this.store = store;
            this.logger = logger;
        }

        public void LoadTemplates(string templateRoot)
        {
            // Registrierte (Standard-)Templates laden
            var instances = store.GetRegistered<ITemplateProxy>();
            templates = instances.Select(t => new TemplateHost(t.GetTemplateCode(), t.TemplateIdentifier, logger)).ToList();

            // Weitere Templates aus Dateien laden
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), templateRoot);
            var info = new DirectoryInfo(path);
            if (!info.Exists)
                return;

            var files = info.GetFiles("*.fpltmpl");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file.FullName);
                templates.Add(new TemplateHost(content, file.Name, logger));
            }
        }

        public ITemplate[] GetTemplates()
            => templates.ToArray();

        public ITemplate[] GetTemplates(string type)
            => templates.Where(t => t.TemplateType == type).ToArray();
    }
}
