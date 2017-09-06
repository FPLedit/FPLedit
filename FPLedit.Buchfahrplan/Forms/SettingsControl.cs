using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
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

namespace FPLedit.Buchfahrplan
{
    public partial class SettingsControl : UserControl, ISaveHandler
    {
        private ISettings settings;
        private BfplAttrs attrs;
        private BfplTemplateChooser chooser;

        public string DisplayName => "Buchfahrplan";

        private SettingsControl()
        {
            InitializeComponent();
        }

        public SettingsControl(Timetable tt, IInfo info) : this()
        {
            settings = info.Settings;
            chooser = new BfplTemplateChooser(info);
            var templates = chooser.AvailableTemplates.Select(t => t.Name).ToArray();
            templateComboBox.Items.AddRange(templates);

            attrs = BfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                cssTextBox.Text = attrs.Css ?? "";

                var tmpl = chooser.GetTemplate(tt);
                templateComboBox.Text = tmpl.Name;
            }
            else
            {
                attrs = new BfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            fontComboBox.Items.AddRange(fontFamilies);

            consoleCheckBox.Checked = settings.Get<bool>("bfpl.console");
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

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.Css = cssTextBox.Text;

            var tmpl_idx = templateComboBox.SelectedIndex;
            var tmpl = chooser.AvailableTemplates[tmpl_idx];
            attrs.Template = chooser.ReduceName(tmpl.GetType().FullName);

            settings.Set("bfpl.console", consoleCheckBox.Checked);
        }
    }
}
