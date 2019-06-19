using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FPLedit
{
    internal class TemplatesForm : Dialog
    {
#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button extractButton, editButton, removeButton, enableButton, disableButton;
#pragma warning restore CS0649

        private readonly TemplateManager manager;
        private ITemplate[] templates;

        private readonly DirectoryInfo templatesDir;

        public TemplatesForm(TemplateManager manager, string templateRoot)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.manager = manager;
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            templatesDir = new DirectoryInfo(Path.Combine(appPath, templateRoot));

            var buildName = new Func<string, string>((x) => x.StartsWith("builtin:") ? "(integriert)" : x);
            gridView.AddColumn<ITemplate>(t => ((TemplateHost)t).Enabled ? "X" : "", "Aktiviert");
            gridView.AddColumn<ITemplate>(t => t.TemplateName, "Name");
            gridView.AddColumn<ITemplate>(t => buildName(t.Identifier), "Dateiname");
            gridView.AddColumn<ITemplate>(t => t.TemplateType, "Typ");

            gridView.SelectedItemsChanged += (s, e) =>
            {
                var tmpl = (TemplateHost)gridView.SelectedItem;
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

            this.AddSizeStateHandler();

            RefreshList();
        }

        private void RefreshList()
        {
            templates = manager.GetAllTemplates();
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

            templatesDir.Create();

            var fn = Path.Combine(templatesDir.FullName, "extracted.fpltmpl");
            fn = FindNextFreeFile(fn);
            File.WriteAllText(fn, src);

            ReloadTemplates();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var res = MessageBox.Show("Die Vorlagendatei wird unwiderruflich gelöscht! Fortfahren?",
                "FPLedit", MessageBoxButtons.OKCancel);
            if (res == DialogResult.Ok)
            {
                if (!templatesDir.Exists)
                    return; // How did we come so far?

                var file = templatesDir.EnumerateFiles(tmpl.Identifier).FirstOrDefault();
                file.Delete();

                ReloadTemplates();
            }
        }

        private void EnableButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];

            var res = MessageBox.Show($"Die Vorlage {tmpl.TemplateName} stammt nicht vom FPLedit-Entwickler. Sie sollten die Vorlage nur aktivieren, wenn Sie " +
                $"sich sicher sein, dass sie aus einer vertrauenswürdigen Quelle stammt. Bösartige Vorlagen könnten möglicherweise Schadcode auf dem System ausführen.",
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
            var dir = Path.GetDirectoryName(fullPath);
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
            if (gridView.SelectedRows.Count() == 0)
                return;

            // May not always work
            var tmpl = templates[gridView.SelectedRow];
            var fn = Path.Combine(templatesDir.FullName, tmpl.Identifier);
            using (var p = new Process())
            {
                p.StartInfo.FileName = fn;

                Stopwatch watch = new Stopwatch();
                watch.Start();
                if (p.Start())
                    p.WaitForExit();
                watch.Stop();

                if (watch.ElapsedMilliseconds < 200)
                    MessageBox.Show($"Es konnte kein Editor gestartet werden! Bitte öffnen Sie die Datei \"{fn}\" in einem Texteditor.", "FPLedit");
            }
            ReloadTemplates();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Close();
    }
}
