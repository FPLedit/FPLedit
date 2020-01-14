﻿using FPLedit.Shared;
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
        private readonly IPluginInterface pluginInterface;
        private readonly List<string> enabledTemplates;
        private List<TemplateHost> templates;

        public TemplateManager(RegisterStore store, IPluginInterface pluginInterface)
        {
            this.store = store;
            this.pluginInterface = pluginInterface;

            enabledTemplates = pluginInterface.Settings.Get("tmpl.enabled", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void LoadTemplates(string templateRoot)
        {
            // Registrierte (Standard-)Templates laden
            var instances = store.GetRegistered<ITemplateProxy>();
            templates = instances.Select(t => new TemplateHost(t.GetTemplateCode(), t.TemplateIdentifier, this.pluginInterface, true)).ToList();

            // Weitere Templates aus Dateien laden
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), templateRoot);
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                return;

            var files = dir.GetFiles("*.fpltmpl");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file.FullName);
                templates.Add(new TemplateHost(content, file.Name, this.pluginInterface, enabledTemplates.Contains(file.Name)));
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
            pluginInterface.Settings.Set("tmpl.enabled", string.Join(";", enabledTemplates));
        }

        internal void DisableTemplate(ITemplate tmpl)
        {
            var fn = Path.GetFileName(tmpl.Identifier);
            enabledTemplates.Remove(fn);
            pluginInterface.Settings.Set("tmpl.enabled", string.Join(";", enabledTemplates));
        }
    }
}
