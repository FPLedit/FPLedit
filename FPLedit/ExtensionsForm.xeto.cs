using Eto.Forms;
using FPLedit.Extensibility;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit
{
    internal sealed class ExtensionsForm : FDialog
    {
        private readonly ExtensionManager manager;
        private readonly IRestartable restartable;

#pragma warning disable CS0649
        private readonly ListBox enabledListBox, disabledListBox;
        private readonly Button deactivateButton, activateButton;
        private readonly Label infoLabel;
#pragma warning restore CS0649

        public ExtensionsForm(ExtensionManager mg, IRestartable restartable)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            manager = mg;
            this.restartable = restartable;

            var enabled_plgs = manager.Plugins.Where(p => p.Enabled);
            var disabled_plgs = manager.Plugins.Where(p => !p.Enabled);

            foreach (var plg in enabled_plgs)
                enabledListBox.Items.Add(new ListItem { Text = plg.Name, Tag = plg });
            enabledListBox.SelectedIndexChanged += (s, a) => ItemSelected(enabledListBox);
            enabledListBox.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = true;
                activateButton.Enabled = false;
            };

            foreach (var plg in disabled_plgs)
                disabledListBox.Items.Add(new ListItem { Text = plg.Name, Tag = plg });
            disabledListBox.SelectedIndexChanged += (s, a) => ItemSelected(disabledListBox);
            disabledListBox.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = false;
                activateButton.Enabled = true;
            };

            this.AddSizeStateHandler();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (manager.EnabledModified)
            {
                manager.WriteConfig();

                var res = MessageBox.Show("Die Änderungen treten erst nach dem nächsten Programmstart in Kraft. Möchten sie das Programm jetzt neu starten?", "FPLedit", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                    restartable.RestartWithCurrentFile();
            }
            Close();
        }

        private void ItemSelected(ListBox lb)
        {
            if (lb.SelectedIndex == -1)
                return;
            var plg = ((ListItem)lb.Items[lb.SelectedIndex]).Tag as PluginInfo;
            if (plg.Author != null)
                infoLabel.Text = "Autor: " + plg.Author + (plg.SecurityContext == SecurityContext.Official ? " [Offizielle Erweiterung]" : "");
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
                    var res = MessageBox.Show($"Die Erweiterung {pluginInfo.Name} stammt nicht vom FPLedit-Entwickler. Sie sollten die Erweiterung nur aktivieren, wenn Sie " +
                        $"sich sicher sein, dass sie aus einer vertrauenswürdigen Quelle stammt. Bösartige Erweiterungen könnten möglicherweise Schadcode auf dem System ausführen.",
                        "Erweiterung aktivieren", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                    if (res == DialogResult.No)
                        return;
                }

                disabledListBox.Items.Remove(item);
                enabledListBox.Items.Add(item);
                enabledListBox.SelectedIndex = -1;
                infoLabel.Text = "";

                manager.Activate(pluginInfo);
            }
        }
    }
}
