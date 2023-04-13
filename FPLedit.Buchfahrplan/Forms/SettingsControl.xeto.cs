using FPLedit.Shared;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.UI;

namespace FPLedit.Buchfahrplan.Forms
{
    internal sealed class SettingsControl : Panel, IAppearanceHandler
    {
        private readonly ISettings settings;
        private readonly BfplAttrs attrs;

#pragma warning disable CS0649,CA2213
        private readonly DropDown templateComboBox = null!;
        private readonly ComboBox fontComboBox = null!;
        private readonly Label exampleLabel = null!, cssLabel = null!;
        private readonly UrlButton cssHelpLinkLabel = null!;
        private readonly CheckBox consoleCheckBox = null!, commentCheckBox = null!, daysCheckBox = null!;
        private readonly TextArea cssTextBox = null!;
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
            attrs.ShowComments = commentCheckBox.Checked ?? false;
            attrs.ShowDays = daysCheckBox.Checked ?? false;

            var tmpl = (ITemplate?)templateComboBox.SelectedValue;
            if (tmpl != null)
                attrs.Template = tmpl.Identifier;

            settings.Set("bfpl.console", consoleCheckBox.Checked ?? false);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
        
        public static class L
        {
            public static readonly string Example = T._("Beispiel");
            public static readonly string Template = T._("Buchfahrplan-Vorlage");
            public static readonly string Font = T._("Schriftart im Buchfahrplan");
            public static readonly string Css = T._("Eigene CSS-Styles");
            public static readonly string CssHelp = T._("Hilfe zu CSS");
            public static readonly string CssHelpLink = T._("https://fahrplan.manuelhu.de/dev/css/");
            public static readonly string ShowComments = T._("Kommentare im Buchfahrplan anzeigen");
            public static readonly string ShowDays = T._("Verkehrstage im Buchfahrplan anzeigen");
            public static readonly string Console = T._("CSS-Test-Konsole bei Vorschau aktivieren (Gilt für alle Fahrpläne)");
        }
    }
}
