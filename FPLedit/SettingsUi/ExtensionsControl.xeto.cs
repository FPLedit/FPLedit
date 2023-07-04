using Eto.Forms;
using FPLedit.Extensibility;
using System;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.SettingsUi
{
    internal sealed class ExtensionsControlHandler : ISettingsControl
    {
        private readonly ExtensionManager mg;
        private readonly IRestartable restartable;

        public ExtensionsControlHandler(ExtensionManager mg, IRestartable restartable)
        {
            this.mg = mg;
            this.restartable = restartable;
        }
        public string DisplayName => T._("Erweiterungen");
        public Control GetControl(IPluginInterface pluginInterface) => new ExtensionsControl(mg, restartable);
    }
    
    internal sealed class ExtensionsControl : Panel
    {
        private readonly ExtensionManager manager;
        private readonly IRestartable restartable;

#pragma warning disable CS0649,CA2213
        private readonly ListBox enabledListBox = default!, disabledListBox = default!;
        private readonly Button deactivateButton = default!, activateButton = default!;
        private readonly Label infoLabel = default!;
        private readonly StackLayout restartStack = default!;
#pragma warning restore CS0649,CA2213

        public ExtensionsControl(ExtensionManager mg, IRestartable restartable)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            manager = mg;
            this.restartable = restartable;

            var enabledPlugins = manager.Plugins.Where(p => p.Enabled && !p.IsBuiltin);
            var disabledPlugins = manager.Plugins.Where(p => !p.Enabled && !p.IsBuiltin);

            foreach (var plg in enabledPlugins)
                enabledListBox.Items.Add(new ListItem { Text = plg.Name, Tag = plg });
            enabledListBox.SelectedIndexChanged += (s, a) => ItemSelected(enabledListBox);
            enabledListBox.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = true;
                activateButton.Enabled = false;
            };

            foreach (var plg in disabledPlugins)
                disabledListBox.Items.Add(new ListItem { Text = plg.Name, Tag = plg });
            disabledListBox.SelectedIndexChanged += (s, a) => ItemSelected(disabledListBox);
            disabledListBox.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = false;
                activateButton.Enabled = true;
            };
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(T._("Möchten sie das Programm jetzt neu starten?"), "FPLedit", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
                restartable.RestartWithCurrentFile();
        }

        private void ItemSelected(ListBox lb)
        {
            if (lb.SelectedIndex == -1)
                return;
            var plg = ((ListItem)lb.Items[lb.SelectedIndex]).Tag as PluginInfo;
            if (plg?.Author != null)
                infoLabel.Text = T._("Autor:") + " " + plg.Author + (plg.SecurityContext == SecurityContext.Official ? " " + T._("[Offizielle Erweiterung]") : "");
            else
                infoLabel.Text = "";
        }

        private void DeactivateButton_Click(object sender, EventArgs e)
        {
            if (enabledListBox.SelectedIndex != -1)
            {
                var item = (ListItem)enabledListBox.Items[enabledListBox.SelectedIndex];
                enabledListBox.Items.Remove(item);
                disabledListBox.Items.Add(item);
                disabledListBox.SelectedIndex = -1;
                infoLabel.Text = "";

                manager.Deactivate((PluginInfo)item.Tag);
                restartStack.Visible = true;
            }
        }

        private void ActivateButton_Click(object sender, EventArgs e)
        {
            if (disabledListBox.SelectedIndex != -1)
            {
                var item = (ListItem)disabledListBox.Items[disabledListBox.SelectedIndex];
                var pluginInfo = (PluginInfo)item.Tag;
                if (pluginInfo.SecurityContext == SecurityContext.ThirdParty)
                {
                    var res = MessageBox.Show(T._("Die Erweiterung {0} stammt nicht vom FPLedit-Entwickler. Sie sollten die Erweiterung nur aktivieren, wenn Sie " +
                        "sich sicher sein, dass sie aus einer vertrauenswürdigen Quelle stammt. Bösartige Erweiterungen könnten möglicherweise Schadcode auf dem System ausführen.", pluginInfo.Name),
                        "Erweiterung aktivieren", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                    if (res == DialogResult.No)
                        return;
                }

                disabledListBox.Items.Remove(item);
                enabledListBox.Items.Add(item);
                enabledListBox.SelectedIndex = -1;
                infoLabel.Text = "";

                manager.Activate(pluginInfo);
                restartStack.Visible = true;
            }
        }

        private static class L
        {
            public static readonly string Activated = T._("Aktiviert");
            public static readonly string Deactivated = T._("Deaktiviert");
            public static readonly string RestartInfo = T._("Die Änderungen treten erst nach dem nächsten Programmstart in Kraft.");
            public static readonly string RestartButton = T._("Jetzt neu starten");
        }
    }
}
