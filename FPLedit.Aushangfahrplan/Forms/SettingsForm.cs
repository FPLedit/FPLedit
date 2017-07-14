using FPLedit.Aushangfahrplan.Model;
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

namespace FPLedit.Aushangfahrplan
{
    public partial class SettingsForm : Form
    {
        private AfplAttrs attrs;
        private AfplTemplateChooser chooser;

        public SettingsForm()
        {
            InitializeComponent();
            chooser = new AfplTemplateChooser();
        }

        public SettingsForm(Timetable tt) : this()
        {
            var templates = chooser.GetAvailableTemplates().Select(t => t.Name).ToArray();
            templateComboBox.Items.AddRange(templates);

            attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                cssTextBox.Text = attrs.Css ?? "";

                var tmpl = chooser.GetTemplate(tt);
                templateComboBox.Text = tmpl.Name;
            }
            else
            {
                attrs = new AfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            fontComboBox.Items.AddRange(fontFamilies);

            consoleCheckBox.Checked = SettingsManager.Get<bool>("afpl.console");
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
            attrs.Font = fontComboBox.Text;
            attrs.Css = cssTextBox.Text;

            var tmpl_idx = templateComboBox.SelectedIndex;
            var tmpl = chooser.GetAvailableTemplates()[tmpl_idx];
            attrs.Template = chooser.ReduceName(tmpl.GetType().FullName);

            SettingsManager.Set("afpl.console", consoleCheckBox.Checked);

            Close();
        }
    }
}
