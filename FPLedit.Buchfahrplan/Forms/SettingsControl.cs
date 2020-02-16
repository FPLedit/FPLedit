using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.UI;

namespace FPLedit.Buchfahrplan.Forms
{
    public class SettingsControl : Panel, IAppearanceHandler
    {
        private readonly ISettings settings;
        private readonly BfplAttrs attrs;

#pragma warning disable CS0649
        private readonly DropDown templateComboBox;
        private readonly ComboBox fontComboBox;
        private readonly Label exampleLabel, cssLabel;
        private readonly UrlButton cssHelpLinkLabel;
        private readonly CheckBox consoleCheckBox, commentCheckBox, daysCheckBox;
        private readonly TextArea cssTextBox;
#pragma warning restore CS0649

        public SettingsControl(Timetable tt, IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = pluginInterface.Settings;
            var chooser = Plugin.GetTemplateChooser(pluginInterface);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);

            attrs = BfplAttrs.GetAttrs(tt) ?? BfplAttrs.CreateAttrs(tt);
            fontComboBox.Text = attrs.Font ?? "";
            cssTextBox.Text = attrs.Css ?? "";
            commentCheckBox.Checked = attrs.ShowComments;
            daysCheckBox.Checked = attrs.ShowDays;

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            consoleCheckBox.Checked = settings.Get<bool>("bfpl.console");
        }

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
