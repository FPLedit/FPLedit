using FPLedit.Shared;
using FPLedit.Aushangfahrplan.Model;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;

namespace FPLedit.Aushangfahrplan.Forms;

internal sealed class SettingsControl : Panel, IAppearanceHandler
{
    private readonly ISettings settings;
    private readonly AfplAttrs attrs;

#pragma warning disable CS0649,CA2213
    private readonly DropDown templateComboBox = default!;
    private readonly ComboBox fontComboBox = default!, hwfontComboBox = default!;
    private readonly Label exampleLabel = default!, hwexampleLabel = default!, cssLabel = default!;
    private readonly UrlButton cssHelpLinkLabel = default!;
    private readonly CheckBox tracksCheckBox = default!, consoleCheckBox = default!, omitTracksSingleCheckBox = default!;
    private readonly TextArea cssTextBox = default!;
#pragma warning restore CS0649,CA2213

    public SettingsControl(IPluginInterface pluginInterface)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        var tt = pluginInterface.Timetable;
        settings = pluginInterface.Settings;
        var chooser = Plugin.GetTemplateChooser(pluginInterface);
        templateComboBox.ItemTextBinding = Binding.Delegate<ITemplate, string>(t => t.TemplateName);
        templateComboBox.DataStore = chooser.AvailableTemplates;

        var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);
        var hwfntComboBox = new FontComboBox(hwfontComboBox, hwexampleLabel);

        attrs = AfplAttrs.GetAttrs(tt) ?? AfplAttrs.CreateAttrs(tt);
        fontComboBox.Text = attrs.Font;
        hwfontComboBox.Text = attrs.HwFont;
        cssTextBox.Text = attrs.Css;
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

    public static class L
    {
        public static readonly string Example = T._("Beispiel");
        public static readonly string Template = T._("Aushangfahrplan-Vorlage");
        public static readonly string Font = T._("Schriftart im Aushangfpl.");
        public static readonly string HwFont = T._("Schriftart in Tabelle");
        public static readonly string Css = T._("Eigene CSS-Styles");
        public static readonly string CssHelp = T._("Hilfe zu CSS");
        public static readonly string CssHelpLink = T._("https://fahrplan.manuelhu.de/dev/css/");
        public static readonly string ShowTracks = T._("Gleisangaben anzeigen");
        public static readonly string OmitSingleTracks = T._("Zeige Gleisangaben nicht, falls Bahnhof nur ein Gleis hat und kein Gleis am Zug definiert ist");
        public static readonly string Console = T._("CSS-Test-Konsole bei Vorschau aktivieren (Gilt für alle Fahrpläne)");
    }
}