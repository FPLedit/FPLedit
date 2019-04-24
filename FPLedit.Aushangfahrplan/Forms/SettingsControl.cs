using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using FPLedit.Aushangfahrplan.Model;
using System.Diagnostics;
using FPLedit.Shared.Ui;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;

namespace FPLedit.Aushangfahrplan.Forms
{
    public partial class SettingsControl : Panel, ISaveHandler, IExpertHandler
    {
        private ISettings settings;
        private AfplAttrs attrs;
        private AfplTemplateChooser chooser;

#pragma warning disable CS0649
        private readonly DropDown templateComboBox;
        private readonly ComboBox fontComboBox, hwfontComboBox;
        private readonly Label exampleLabel, hwexampleLabel, cssLabel;
        private readonly LinkButton cssHelpLinkLabel;
        private readonly CheckBox tracksCheckBox, consoleCheckBox;
        private readonly TextArea cssTextBox;
#pragma warning restore CS0649

        public SettingsControl(Timetable tt, IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = info.Settings;
            chooser = new AfplTemplateChooser(info);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);
            var hwfntComboBox = new FontComboBox(hwfontComboBox, hwexampleLabel);

            attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                hwfontComboBox.Text = attrs.HwFont;
                cssTextBox.Text = attrs.Css ?? "";
                tracksCheckBox.Checked = attrs.ShowTracks;
            }
            else
            {
                attrs = new AfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            consoleCheckBox.Checked = settings.Get<bool>("afpl.console");
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/dev/css/");

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.HwFont = hwfontComboBox.Text;
            attrs.Css = cssTextBox.Text;
            attrs.ShowTracks = tracksCheckBox.Checked.Value;

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
