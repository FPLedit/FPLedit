using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using FPLedit.Aushangfahrplan.Model;
using System.Drawing.Text;
using System.Diagnostics;
using FPLedit.Shared.Ui;
using Eto.Forms;
using FPLedit.Shared.Templating;
using Eto.Drawing;

namespace FPLedit.Aushangfahrplan.Forms
{
    public partial class SettingsControl : Panel, ISaveHandler, IExpertHandler
    {
        private ISettings settings;
        private AfplAttrs attrs;
        private AfplTemplateChooser chooser;

#pragma warning disable CS0649
        private DropDown templateComboBox;
        private ComboBox fontComboBox, hwfontComboBox;
        private Label exampleLabel, hwexampleLabel, cssLabel;
        private LinkButton cssHelpLinkLabel;
        private CheckBox consoleCheckBox;
        private TextArea cssTextBox;
#pragma warning restore CS0649

        public SettingsControl(Timetable tt, IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = info.Settings;
            chooser = new AfplTemplateChooser(info);
            templateComboBox.DataStore = chooser.AvailableTemplates;
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);

            attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                hwfontComboBox.Text = attrs.HwFont;
                cssTextBox.Text = attrs.Css ?? "";
            }
            else
            {
                attrs = new AfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            fontComboBox.DataStore = fontFamilies;
            hwfontComboBox.DataStore = fontFamilies;
            fontComboBox.ItemTextBinding = Binding.Property<string, string>(s => s);
            hwfontComboBox.ItemTextBinding = Binding.Property<string, string>(s => s);
            fontComboBox.TextChanged += fontComboBox_TextChanged;
            hwfontComboBox.TextChanged += hwfontComboBox_TextChanged;

            consoleCheckBox.Checked = settings.Get<bool>("afpl.console");
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/dev/css/");

        private void fontComboBox_TextChanged(object sender, EventArgs e)
           => exampleLabel.Font = new Font(fontComboBox.Text, 10);

        private void hwfontComboBox_TextChanged(object sender, EventArgs e)
            => hwexampleLabel.Font = new Font(hwfontComboBox.Text, 10);

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.HwFont = hwfontComboBox.Text;
            attrs.Css = cssTextBox.Text;

            var tmpl = (ITemplate)templateComboBox.SelectedValue;
            if (tmpl != null)
                attrs.Template = tmpl.Identifier;

            settings.Set("afpl.console", consoleCheckBox.Checked.Value);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
    }
}
