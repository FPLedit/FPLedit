using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using System.Diagnostics;
using FPLedit.Shared.Ui;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.UI;

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
        private CheckBox consoleCheckBox, commentCheckBox, daysCheckBox;
        private TextArea cssTextBox;
#pragma warning restore CS0649
        private FontComboBox fntComboBox;

        public SettingsControl(Timetable tt, IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = info.Settings;
            chooser = new BfplTemplateChooser(info);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            fntComboBox = new FontComboBox(fontComboBox, exampleLabel);

            attrs = BfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                cssTextBox.Text = attrs.Css ?? "";
                commentCheckBox.Checked = attrs.ShowComments;
                daysCheckBox.Checked = attrs.ShowDays;
            }
            else
            {
                attrs = new BfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            consoleCheckBox.Checked = settings.Get<bool>("bfpl.console");
        }

        private void cssHelpLinkLabel_LinkClicked(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/dev/css/");

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.Css = cssTextBox.Text;
            attrs.ShowComments = commentCheckBox.Checked.Value;
            attrs.ShowDays = daysCheckBox.Checked.Value;

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
