using System.Collections.Generic;
using FPLedit.Shared;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Kursbuch.Model;
using FPLedit.Shared.UI;

namespace FPLedit.Kursbuch.Forms
{
    internal sealed class SettingsControl : Panel, IAppearanceHandler
    {
        private readonly ISettings settings;
        private readonly KfplAttrs attrs;

        private const string NO_KBS_TEXT = "<keine Nummer>";

#pragma warning disable CS0649,CA2213
        private readonly DropDown templateComboBox = default!;
        private readonly ComboBox fontComboBox = default!, hefontComboBox = default!;
        private readonly Label exampleLabel = default!, heexampleLabel = default!, cssLabel = default!, kbsnLabel = default!;
        private readonly UrlButton cssHelpLinkLabel = default!;
        private readonly CheckBox consoleCheckBox = default!;
        private readonly TextArea cssTextBox = default!;
        private readonly GridView kbsnListView = default!;
#pragma warning restore CS0649,CA2213

        private readonly Dictionary<int, string> setRouteNumbers;

        public SettingsControl(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            var tt = pluginInterface.Timetable;
            settings = pluginInterface.Settings;
            var chooser = Plugin.GetTemplateChooser(pluginInterface);
            templateComboBox.ItemTextBinding = Binding.Delegate<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);
            var hefntComboBox = new FontComboBox(hefontComboBox, heexampleLabel);

            attrs = KfplAttrs.GetAttrs(tt) ?? KfplAttrs.CreateAttrs(tt);
            fontComboBox.Text = attrs.Font;
            hefontComboBox.Text = attrs.HeFont;
            cssTextBox.Text = attrs.Css ?? "";

            setRouteNumbers = new Dictionary<int, string>();
#pragma warning disable CA2000
            kbsnListView.AddColumn(new TextBoxCell
                {
                    Binding = Binding.Delegate<Route, string>(r =>
                    {
                        setRouteNumbers.TryGetValue(r.Index, out string val);
                        return val ?? attrs.KBSn.GetValue(r.Index) ?? NO_KBS_TEXT;
                    },
                    (r, n) => setRouteNumbers[r.Index] = n)
                }, "Name", editable: true
            );
#pragma warning restore CA2000
            kbsnListView.AddFuncColumn<Route>(r => r.GetRouteName(), T._("Strecke"));
            kbsnListView.DataStore = tt.GetRoutes();

            var tmpl = chooser.GetTemplate(tt);
            templateComboBox.SelectedValue = tmpl;

            consoleCheckBox.Checked = settings.Get<bool>("kfpl.console");

            Shown += (s, e) =>
            {
                if (!Eto.Platform.Instance.IsWpf)
                    kbsnLabel.WordWrap(200);
            };
        }

        public void Save()
        {
            attrs.Font = fontComboBox.Text;
            attrs.HeFont = hefontComboBox.Text;
            attrs.Css = cssTextBox.Text;

            foreach (var itm in setRouteNumbers)
            {
                var kbs = itm.Value;
                if (kbs == NO_KBS_TEXT)
                    continue;
                attrs.KBSn.SetValue(itm.Key, kbs);
            }

            var tmpl = (ITemplate)templateComboBox.SelectedValue;
            if (tmpl != null)
                attrs.Template = tmpl.Identifier;

            settings.Set("kfpl.console", consoleCheckBox.Checked!.Value);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var col in kbsnListView.Columns)
                    if (!col.IsDisposed)
                        col.Dispose();
            
            base.Dispose(disposing);
        }
        
        public static class L
        {
            public static readonly string Example = T._("Beispiel");
            public static readonly string Template = T._("Tabellenfahrplan-Vorlage");
            public static readonly string Font = T._("Schriftart im Tabellenfahrpl.");
            public static readonly string StrFont = T._("Schriftart für Kursbuchstr.");
            public static readonly string Css = T._("Eigene CSS-Styles");
            public static readonly string CssHelp = T._("Hilfe zu CSS");
            public static readonly string CssHelpLink = T._("https://fahrplan.manuelhu.de/dev/css/");
            public static readonly string Numbers = T._("Kursbuchstreckennummern");
            public static readonly string NumbersDescription = T._("Zum Bearbeiten zwei Mal mit Kurzem Abstand auf den Eintrag klicken");
            public static readonly string Console = T._("CSS-Test-Konsole bei Vorschau aktivieren (Gilt für alle Fahrpläne)");
        }
    }
}
