using FPLedit.Shared;
using FPLedit.Aushangfahrplan.Model;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;

namespace FPLedit.Aushangfahrplan.Forms
{
    internal sealed class SettingsControl : Panel, IAppearanceHandler
    {
        private readonly ISettings settings;
        private readonly AfplAttrs attrs;

#pragma warning disable CS0649
        private readonly DropDown templateComboBox;
        private readonly ComboBox fontComboBox, hwfontComboBox;
        private readonly Label exampleLabel, hwexampleLabel, cssLabel;
        private readonly UrlButton cssHelpLinkLabel;
        private readonly CheckBox tracksCheckBox, consoleCheckBox, omitTracksSingleCheckBox;
        private readonly TextArea cssTextBox;
#pragma warning restore CS0649

        public SettingsControl(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            var tt = pluginInterface.Timetable;
            settings = pluginInterface.Settings;
            var chooser = Plugin.GetTemplateChooser(pluginInterface);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);
            var hwfntComboBox = new FontComboBox(hwfontComboBox, hwexampleLabel);

            attrs = AfplAttrs.GetAttrs(tt) ?? AfplAttrs.CreateAttrs(tt);
            fontComboBox.Text = attrs.Font;
            hwfontComboBox.Text = attrs.HwFont;
            cssTextBox.Text = attrs.Css ?? "";
            tracksCheckBox.Checked = attrs.ShowTracks;

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            consoleCheckBox.Checked = settings.Get<bool>("afpl.console");

            omitTracksSingleCheckBox.Checked = attrs.OmitTracksWhenSingle;
        }

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.HwFont = hwfontComboBox.Text;
            attrs.Css = cssTextBox.Text;
            attrs.ShowTracks = tracksCheckBox.Checked ?? false;
            attrs.OmitTracksWhenSingle = omitTracksSingleCheckBox.Checked ?? false;

            var tmpl = (ITemplate)templateComboBox.SelectedValue;
            if (tmpl != null)
                attrs.Template = tmpl.Identifier;

            settings.Set("afpl.console", consoleCheckBox.Checked ?? false);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
    }
}
