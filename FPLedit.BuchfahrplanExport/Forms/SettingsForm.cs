using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class SettingsForm : Form
    {
        private BFPL_Attrs data;
        private BfplTemplateChooser chooser;

        public SettingsForm()
        {
            InitializeComponent();
            chooser = new BfplTemplateChooser();
        }

        public SettingsForm(Timetable tt) : this()
        {
            var templates = chooser.GetAvailableTemplates().Select(t => t.Name).ToArray();
            templateComboBox.Items.AddRange(templates);

            var dataEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (dataEn != null)
            {
                data = new BFPL_Attrs(dataEn, tt);
                fontComboBox.Text = data.Font;
                cssTextBox.Text = data.Css ?? "";

                var typeName = chooser.ExpandName(data.Template);
                var tmpl = chooser.GetAvailableTemplates().FirstOrDefault(t => t.GetType().FullName == typeName) ?? new Templates.BuchfahrplanTemplate();
                templateComboBox.Text = tmpl.Name;
            }
            else
            {
                data = new BFPL_Attrs(tt);
                tt.Children.Add(data.XMLEntity);
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            fontComboBox.Items.AddRange(fontFamilies);

            consoleCheckBox.Checked = bool.Parse(SettingsManager.Get("bfpl.console", "false"));
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/buchfahrplaene/css/");

        private void fontComboBox_TextChanged(object sender, EventArgs e)
             => exampleLabel.Font = new Font(fontComboBox.Text, 10);

        private void cssTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Tab-Width anpassen
            int tabWidth = 4;
            if (e.KeyCode == Keys.Tab)
            {
                cssTextBox.SelectedText = new string(' ', tabWidth);
                e.SuppressKeyPress = true;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            data.Font = fontComboBox.Text;
            data.Css = cssTextBox.Text;

            var tmpl_idx = templateComboBox.SelectedIndex;
            var tmpl = chooser.GetAvailableTemplates()[tmpl_idx];
            data.Template = chooser.ReduceName(tmpl.GetType().FullName);

            SettingsManager.Set("bfpl.console", consoleCheckBox.Checked.ToString());

            Close();
        }
    }
}
