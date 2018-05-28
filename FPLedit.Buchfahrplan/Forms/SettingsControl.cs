using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using System.Drawing.Text;
using System.Diagnostics;
using FPLedit.Shared.Ui;
using Eto.Forms;
using FPLedit.Shared.Templating;
using Eto.Drawing;
using FPLedit.Buchfahrplan;
using FPLedit.Buchfahrplan.Model;

namespace FPLedit.Buchfahrplan.Forms
{
    public partial class SettingsControl : Panel, ISaveHandler, IExpertHandler
    {
        private ISettings settings;
        private BfplAttrs attrs;
        private BfplTemplateChooser chooser;

#pragma warning disable CS0649
        private DropDown templateComboBox;
        private ComboBox fontComboBox;
        private Label exampleLabel, cssLabel;
        private LinkButton cssHelpLinkLabel;
        private CheckBox consoleCheckBox;
        private TextArea cssTextBox;
#pragma warning restore CS0649

        public SettingsControl(Timetable tt, IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = info.Settings;
            chooser = new BfplTemplateChooser(info);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            attrs = BfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                cssTextBox.Text = attrs.Css ?? "";
            }
            else
            {
                attrs = new BfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).OrderBy(f => f).ToArray();
            fontComboBox.DataStore = fontFamilies;
            fontComboBox.ItemTextBinding = Binding.Property<string, string>(s => s);
            fontComboBox.TextChanged += fontComboBox_TextChanged;

            consoleCheckBox.Checked = settings.Get<bool>("bfpl.console");
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/dev/css/");

        private void fontComboBox_TextChanged(object sender, EventArgs e)
        {
            if (fontComboBox.Text == "")
                return;

            try
            {
                exampleLabel.Font = new Font(fontComboBox.Text, 10);
            }
            catch { }
        }

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.Css = cssTextBox.Text;

            var tmpl = (ITemplate)templateComboBox.SelectedValue;
            if (tmpl != null)
                attrs.Template = tmpl.Identifier;

            settings.Set("bfpl.console", consoleCheckBox.Checked.Value);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
    }
}
