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
        private readonly RegisterStore store;
        private readonly ILog logger;
        private readonly ISettings settings;
        private readonly List<string> enabledTemplates;
        private List<TemplateHost> templates;

        public TemplateManager(RegisterStore store, ILog logger, ISettings settings)
        {
            this.store = store;
            this.logger = logger;
            this.settings = settings;

            try
            {
                if (Directory.Exists(TemplateCompiler.CompilerTemp))
                    Directory.Delete(TemplateCompiler.CompilerTemp, true);
                Directory.CreateDirectory(TemplateCompiler.CompilerTemp);
            }
            catch { }

            enabledTemplates = settings.Get("tmpl.enabled", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void LoadTemplates(string templateRoot)
        {
            // Registrierte (Standard-)Templates laden
            var instances = store.GetRegistered<ITemplateProxy>();
            templates = instances.Select(t => new TemplateHost(t.GetTemplateCode(), t.TemplateIdentifier, logger, true)).ToList();

            // Weitere Templates aus Dateien laden
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), templateRoot);
            var info = new DirectoryInfo(path);
            if (!info.Exists)
                return;

            var files = info.GetFiles("*.fpltmpl");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file.FullName);
                templates.Add(new TemplateHost(content, file.Name, logger, enabledTemplates.Contains(file.Name)));
            }
        }

        public ITemplate[] GetAllTemplates()
            => templates.ToArray();

        public ITemplate[] GetTemplates(string type)
            => templates.Where(t => t.Enabled && t.TemplateType == type).ToArray();

        internal void DebugCompileAll()
        {
            var tt = new Timetable(TimetableType.Linear);
            foreach (var t in templates)
                t.GenerateResult(tt);
        }

        internal void EnableTemplate(ITemplate tmpl)
        {
            var fn = Path.GetFileName(tmpl.Identifier);
            if (enabledTemplates.Contains(fn))
                return;
            enabledTemplates.Add(fn);
            settings.Set("tmpl.enabled", string.Join(";", enabledTemplates));
        }

        internal void DisableTemplate(ITemplate tmpl)
        {
            var fn = Path.GetFileName(tmpl.Identifier);
            enabledTemplates.Remove(fn);
            settings.Set("tmpl.enabled", string.Join(";", enabledTemplates));
        }
    }
}
