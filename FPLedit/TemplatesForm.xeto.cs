using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Templating;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit
{
    internal class TemplatesForm : Dialog
    {
#pragma warning disable CS0649
        private GridView gridView;
        private Button extractButton, editButton, removeButton;
#pragma warning restore CS0649

        private TemplateManager manager;
        private ITemplate[] templates;

        private DirectoryInfo templatesDir, deactivatedDir;

        public TemplatesForm(TemplateManager manager, string templateRoot)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.manager = manager;
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            templatesDir = new DirectoryInfo(Path.Combine(appPath, templateRoot));
            deactivatedDir = new DirectoryInfo(Path.Combine(appPath, templateRoot, "deactivated"));

            var buildName = new Func<string, string>((x) => x.StartsWith("builtin:") ? "(integriert)" : x);
            gridView.AddColumn<ITemplate>(t => t.TemplateName, "Name");
            gridView.AddColumn<ITemplate>(t => buildName(t.Identifier), "Dateiname");
            gridView.AddColumn<ITemplate>(t => t.TemplateType, "Typ");

            gridView.SelectedItemsChanged += (s, e) =>
            {
                var tmpl = (ITemplate)gridView.SelectedItem;
                if (tmpl == null)
                {
                    extractButton.Enabled = editButton.Enabled = removeButton.Enabled = false;
                    return;
                }
                var builtin = tmpl.Identifier.StartsWith("builtin:");
                extractButton.Enabled = builtin;
                editButton.Enabled = removeButton.Enabled = !builtin;
            };

            this.AddSizeStateHandler();

            RefreshList();
        }

        private void RefreshList()
        {
            templates = manager.GetTemplates();
            gridView.DataStore = templates;
        }

        private void ReloadTemplates()
        {
            manager.LoadTemplates(templatesDir.FullName);
            RefreshList();
        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var src = tmpl.TemplateSource;

            templatesDir.Create();

            var fn = Path.Combine(templatesDir.FullName, "extracted.fpltmpl");
            fn = FindNextFreeFile(fn);
            File.WriteAllText(fn, src);

            ReloadTemplates();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var res = MessageBox.Show("Die Vorlagendatei wird verschoben und muss zur Reaktivierung manuell wieder zurück in den templates-Ordner im Installationsverzeichnis verschoben werden!",
                "FPLedit", MessageBoxButtons.OKCancel);
            if (res == DialogResult.Ok)
            {
                if (!templatesDir.Exists)
                    return; // How did we come so far?
                deactivatedDir.Create(); // Ensure we have a destination

                var file = templatesDir.EnumerateFiles(tmpl.Identifier).FirstOrDefault();
                var dest = Path.Combine(deactivatedDir.FullName, tmpl.Identifier);
                dest = FindNextFreeFile(dest);
                file.MoveTo(dest);

                ReloadTemplates();
            }
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

        private void editButton_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count() == 0)
                return;

            // May not always work
            var tmpl = templates[gridView.SelectedRow];
            var fn = Path.Combine(templatesDir.FullName, tmpl.Identifier);
            Process p = new Process();
            p.StartInfo.FileName = fn;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (p.Start())
                p.WaitForExit();
            watch.Stop();
            if (watch.ElapsedMilliseconds < 200)
                MessageBox.Show($"Es konnte kein Editor gestartet werden! Bitte öffnen Sie die Datei \"{fn}\" in einem Texteditor.", "FPLedit");
            ReloadTemplates();
        }

        private void closeButton_Click(object sender, EventArgs e) => Close();
    }
}
