#nullable enable
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Templating;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.SettingsUi
{
    internal sealed class TemplatesControlHandler : ISettingsControl
    {
        public string DisplayName => T._("Vorlagen");
        public Control GetControl(IPluginInterface pluginInterface) => new TemplatesControl((TemplateManager)pluginInterface.TemplateManager);
    }
    
    internal sealed class TemplatesControl : Panel
    {
#pragma warning disable CS0649,CA2213
        private readonly GridView gridView = default!;
        private readonly Button extractButton = default!, editButton = default!, removeButton = default!, enableButton = default!, disableButton = default!;
#pragma warning restore CS0649,CA2213

        private readonly TemplateManager manager;
        private TemplateHost[] templates = Array.Empty<TemplateHost>();

        private readonly DirectoryInfo templatesDir;

        public TemplatesControl(TemplateManager manager)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.manager = manager;
            templatesDir = new DirectoryInfo(Path.Combine(PathManager.Instance.AppDirectory, manager.TemplatePath));

            var buildName = new Func<string, string>((x) => x.StartsWith("builtin:") ? T._("(integriert)") : x);
            gridView.AddCheckColumn<TemplateHost>(t => t.Enabled, T._("Aktiviert"));
            gridView.AddFuncColumn<TemplateHost>(t => t.TemplateName, T._("Name"));
            gridView.AddFuncColumn<TemplateHost>(t => buildName(t.Identifier), T._("Dateiname"));
            gridView.AddFuncColumn<TemplateHost>(t => t.TemplateType!, T._("Typ"));

            gridView.SelectedItemsChanged += (_, _) =>
            {
                var tmpl = (TemplateHost?)gridView.SelectedItem;
                if (tmpl == null)
                {
                    extractButton.Enabled = editButton.Enabled = removeButton.Enabled = enableButton.Enabled = disableButton.Enabled = false;
                    return;
                }
                var builtin = tmpl.Identifier.StartsWith("builtin:");
                extractButton.Enabled = builtin;
                editButton.Enabled = removeButton.Enabled = !builtin;
                enableButton.Enabled = !builtin && !tmpl.Enabled;
                disableButton.Enabled = !builtin && tmpl.Enabled;
            };

            RefreshList();
        }

        private void RefreshList()
        {
            templates = manager.GetAllTemplates().Cast<TemplateHost>().ToArray();
            gridView.DataStore = templates;
        }

        private void ReloadTemplates()
        {
            manager.LoadTemplates(templatesDir.FullName);
            RefreshList();
        }

        private void ExtractButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var src = tmpl.TemplateSource;
            if (src == null) return;

            templatesDir.Create();

            var fn = Path.Combine(templatesDir.FullName, "extracted.fpltmpl");
            fn = FindNextFreeFile(fn);
            File.WriteAllText(fn, src);

            ReloadTemplates();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var res = MessageBox.Show(T._("Die Vorlagendatei wird unwiderruflich gelöscht! Fortfahren?"),
                "FPLedit", MessageBoxButtons.OKCancel);
            if (res == DialogResult.Ok)
            {
                if (!templatesDir.Exists)
                    return; // How did we come so far?

                var file = templatesDir.EnumerateFiles(tmpl.Identifier).FirstOrDefault();
                file?.Delete();

                ReloadTemplates();
            }
        }

        private void EnableButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];

            var res = MessageBox.Show(T._("Die Vorlage {0} stammt nicht vom FPLedit-Entwickler. Sie sollten die Vorlage nur aktivieren, wenn Sie " +
                "sich sicher sein, dass sie aus einer vertrauenswürdigen Quelle stammt. Bösartige Vorlagen könnten möglicherweise Schadcode auf dem System ausführen.", tmpl.TemplateName),
                "Vorlage aktivieren", MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (res == DialogResult.No)
                return;

            if (!templatesDir.Exists)
                return; // How did we come so far?

            manager.EnableTemplate(tmpl);

            ReloadTemplates();
        }

        private void DisableButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];

            if (!templatesDir.Exists)
                return; // How did we come so far?

            manager.DisableTemplate(tmpl);

            ReloadTemplates();
        }

        // Doppelte Dateinamen vermeiden, indem eine Nummer angehängt wird.
        private string FindNextFreeFile(string fullPath)
        {
            if (!File.Exists(fullPath))
                return fullPath;
            var dir = Path.GetDirectoryName(fullPath)!;
            var ext = Path.GetExtension(fullPath);
            var basename = Path.GetFileNameWithoutExtension(fullPath);
            int i = 1;
            while (true)
            {
                var name = basename + i + ext;
                var filename = Path.Combine(dir, name);
                if (!File.Exists(filename))
                    return filename;
                i++;
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (!gridView.SelectedRows.Any())
                return;

            // May not always work
            var tmpl = templates[gridView.SelectedRow];
            var fn = Path.Combine(templatesDir.FullName, tmpl.Identifier);
            
            var watch = new Stopwatch();
            using (var p = OpenHelper.OpenProc(fn))
            {
                if (p != null)
                {
                    watch.Start();
                    p.WaitForExit();
                    watch.Stop();
                }
                else
                    MessageBox.Show(T._("Es konnte kein Editor gestartet werden! Bitte öffnen Sie die Datei \"{0}\" in einem Texteditor.", fn), "FPLedit");
            }
            ReloadTemplates();
        }
        
        private static class L
        {
            public static readonly string EditButton = T._("&Bearbeiten");
            public static readonly string ExtractButton = T._("Integrierte Vorlage &extrahieren");
            public static readonly string DeleteButton = T._("&Löschen");
            public static readonly string ActivateButton = T._("&Aktivieren");
            public static readonly string DeactivateButton = T._("&Deaktivieren");
        }
    }
}
