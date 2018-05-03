using Eto.Forms;
using FPLedit.Shared.Templating;
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
            gridView.Columns.Add(new GridColumn() {
                DataCell = new TextBoxCell { Binding = Binding.Property<ITemplate, string>(t => t.TemplateName) },
                HeaderText = "Name"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<ITemplate, string>(t => buildName(t.Identifier)) },
                HeaderText = "Dateiname"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<ITemplate, string>(t => t.TemplateType) },
                HeaderText = "Typ"
            });

            RefreshList();
        }

        private void RefreshList()
        {
            templates = manager.GetTemplates();
            gridView.DataStore = templates;
        }

        private void ReloadTemplates()
        {
            manager.LoadTemplates("templates");
            RefreshList();
        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[gridView.SelectedRow];
            var src = tmpl.TemplateSource;

            templatesDir.Create();
            var fn = Path.Combine(templatesDir.FullName, "extracted.fpltmpl");
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

            if (p.Start())
                p.WaitForExit();
            ReloadTemplates();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
