using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using System.Diagnostics;
using FPLedit.Shared.Ui;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Kursbuch.Model;
using FPLedit.Shared.UI;

namespace FPLedit.Kursbuch.Forms
{
    public partial class SettingsControl : Panel, ISaveHandler, IExpertHandler
    {
        private readonly ISettings settings;
        private readonly KfplAttrs attrs;
        private readonly KfplTemplateChooser chooser;

        private const string NO_KBS_TEXT = "<keine Nummer>";

#pragma warning disable CS0649
        private readonly DropDown templateComboBox;
        private readonly ComboBox fontComboBox, hefontComboBox;
        private readonly Label exampleLabel, heexampleLabel, cssLabel, kbsnLabel;
        private readonly UrlButton cssHelpLinkLabel;
        private readonly CheckBox consoleCheckBox;
        private readonly TextArea cssTextBox;
        private readonly GridView kbsnListView;
#pragma warning restore CS0649

        private readonly Dictionary<int, string> setRouteNumbers;

        public SettingsControl(Timetable tt, IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            settings = pluginInterface.Settings;
            chooser = new KfplTemplateChooser(pluginInterface);
            templateComboBox.ItemTextBinding = Binding.Property<ITemplate, string>(t => t.TemplateName);
            templateComboBox.DataStore = chooser.AvailableTemplates;

            var fntComboBox = new FontComboBox(fontComboBox, exampleLabel);
            var hefntComboBox = new FontComboBox(hefontComboBox, heexampleLabel);

            attrs = KfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                fontComboBox.Text = attrs.Font;
                hefontComboBox.Text = attrs.HeFont;
                cssTextBox.Text = attrs.Css ?? "";
            }
            else
            {
                attrs = new KfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            setRouteNumbers = new Dictionary<int, string>();
            var col = kbsnListView.AddColumn(new TextBoxCell
                {
                    Binding = Binding.Delegate<Route, string>(r =>
                    {
                        setRouteNumbers.TryGetValue(r.Index, out string val);
                        return val ?? attrs.KBSn.GetValue(r.Index) ?? NO_KBS_TEXT;
                    },
                    (r, n) => setRouteNumbers[r.Index] = n)
                }, "Name"
            );
            col.Editable = true;
            kbsnListView.AddColumn<Route>(r => r.GetRouteName(), "Dateiname");
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

            settings.Set("kfpl.console", consoleCheckBox.Checked.Value);
        }

        public void SetExpertMode(bool enabled)
        {
            cssTextBox.Visible = cssLabel.Visible = cssHelpLinkLabel.Visible = consoleCheckBox.Visible = enabled;
        }
    }
}
