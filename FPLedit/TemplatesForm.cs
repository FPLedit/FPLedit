using FPLedit.Shared.Templating;
using FPLedit.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    internal partial class TemplatesForm : Form
    {
        private TemplateManager manager;
        private ITemplate[] templates;

        private DirectoryInfo templatesDir, deactivatedDir;

        public TemplatesForm()
        {
            InitializeComponent();

            listView.Columns.Add("Name");
            listView.Columns.Add("Dateiname");
            listView.Columns.Add("Typ");
        }

        public TemplatesForm(TemplateManager manager, string templateRoot) : this()
        {
            this.manager = manager;
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            templatesDir = new DirectoryInfo(Path.Combine(appPath, templateRoot));
            deactivatedDir = new DirectoryInfo(Path.Combine(appPath, templateRoot, "deactivated"));
        }

        private void RefreshList()
        {
            var buildName = new Func<string, string>((x) => x.StartsWith("builtin:") ? "(integriert)" : x);

            templates = manager.GetTemplates();
            var templs = templates.Select(t => new string[] { t.TemplateName, buildName(t.Identifier), t.TemplateType }).ToArray();
            listView.Items.Clear();
            foreach (var t in templs)
                listView.Items.Add(new ListViewItem(t));

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ReloadTemplates()
        {
            manager.LoadTemplates("templates");
            RefreshList();
        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[listView.SelectedIndices[0]];
            var src = tmpl.TemplateSource;

            templatesDir.Create();
            var fn = Path.Combine(templatesDir.FullName, "extracted.fpltmpl");
            File.WriteAllText(fn, src);

            ReloadTemplates();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var tmpl = templates[listView.SelectedIndices[0]];
            var res = MessageBox.Show("Die Vorlagendatei wird verschoben und muss zur Reaktivierung manuell wieder zurück in den templates-Ordner im Installationsverzeichnis verschoben werden!",
                "FPLedit", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
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
            if (listView.SelectedIndices.Count == 0)
                return;

            // May not always work
            var tmpl = templates[listView.SelectedIndices[0]];
            var fn = Path.Combine(templatesDir.FullName, tmpl.Identifier);
            Process p = new Process();
            p.StartInfo.FileName = fn;

            if(p.Start())
                p.WaitForExit();
            ReloadTemplates();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count > 0)
            {
                bool isBuiltin = templates[listView.SelectedIndices[0]].Identifier.StartsWith("builtin:");
                extractButton.Enabled = isBuiltin;
                removeButton.Enabled = editButton.Enabled = !isBuiltin;
            }
            else
                extractButton.Enabled = removeButton.Enabled = editButton.Enabled = false;
        }

        private void closeButton_Click(object sender, EventArgs e)
            => Close();

        private void TemplatesForm_Load(object sender, EventArgs e)
            => RefreshList();
    }
}
