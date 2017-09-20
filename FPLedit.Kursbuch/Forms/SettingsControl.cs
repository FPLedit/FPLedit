using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FPLedit.Shared;
using FPLedit.Kursbuch.Model;
using System.Drawing.Text;
using System.Diagnostics;
using FPLedit.Shared.Ui;

namespace FPLedit.Kursbuch.Forms
{
    public partial class SettingsControl : UserControl, ISaveHandler, IExpertHandler
    {
        private ISettings settings;
        private KfplAttrs attrs;
        private KfplTemplateChooser chooser;

        private SettingsControl()
        {
            InitializeComponent();
        }

        public SettingsControl(Timetable tt, IInfo info) : this()
        {
            settings = info.Settings;
            chooser = new KfplTemplateChooser(info);
            var templates = chooser.AvailableTemplates.Select(t => t.Name).ToArray();
            templateComboBox.Items.AddRange(templates);

            attrs = KfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                cssTextBox.Text = attrs.Css ?? "";
                kbsTextBox.Text = attrs.Kbs;
            }
            else
            {
                attrs = new KfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.Text = tmpl.Name;
        }

        private void SettingsControl_Load(object sender, EventArgs e)
        {
            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            fontComboBox.Items.AddRange(fontFamilies);
            hwfontComboBox.Items.AddRange(fontFamilies);

            consoleCheckBox.Checked = settings.Get<bool>("kfpl.console");
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/tabellenfahrplaene/css/");

        private void fontComboBox_TextChanged(object sender, EventArgs e)
           => exampleLabel.Font = new Font(fontComboBox.Text, 10);

        private void hwfontComboBox_TextChanged(object sender, EventArgs e)
            => hwexampleLabel.Font = new Font(hwfontComboBox.Text, 10);

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
            attrs.HeFont = hwfontComboBox.Text;
            attrs.Css = cssTextBox.Text;
            attrs.Kbs = kbsTextBox.Text;

            var tmpl_idx = templateComboBox.SelectedIndex;
            if (tmpl_idx != -1)
            {
                var tmpl = chooser.AvailableTemplates[tmpl_idx];
                attrs.Template = chooser.ReduceName(tmpl.GetType().FullName);
            }

            settings.Set("kfpl.console", consoleCheckBox.Checked);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
    }
}
