﻿using Eto.Forms;
using FPLedit.Extensibility;
using System;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.SettingsUi
{
    internal sealed class ExtensionsFormHandler : ISettingsControl
    {
        private readonly ExtensionManager mg;
        private readonly IRestartable restartable;

        public ExtensionsFormHandler(ExtensionManager mg, IRestartable restartable)
        {
            this.mg = mg;
            this.restartable = restartable;
        }
        public string DisplayName => LocalizationHelper._("Erweiterungen");
        public Control GetControl(IPluginInterface pluginInterface) => new ExtensionsForm(mg, restartable);
    }
    
    internal sealed class ExtensionsForm : Panel
    {
        private readonly ExtensionManager manager;
        private readonly IRestartable restartable;

#pragma warning disable CS0649
        private readonly ListBox enabledListBox, disabledListBox;
        private readonly Button deactivateButton, activateButton;
        private readonly Label infoLabel;
        private readonly StackLayout restartStack;
#pragma warning restore CS0649

        public ExtensionsForm(ExtensionManager mg, IRestartable restartable)
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
            var res = MessageBox.Show(LocalizationHelper._("Möchten sie das Programm jetzt neu starten?"), "FPLedit", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
                restartable.RestartWithCurrentFile();
        }

        private void ItemSelected(ListBox lb)
        {
            if (lb.SelectedIndex == -1)
                return;
            var plg = ((ListItem)lb.Items[lb.SelectedIndex]).Tag as PluginInfo;
            if (plg.Author != null)
                infoLabel.Text = LocalizationHelper._("Autor:") + " " + plg.Author + (plg.SecurityContext == SecurityContext.Official ? " " + LocalizationHelper._("[Offizielle Erweiterung]") : "");
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
                    var res = MessageBox.Show(LocalizationHelper._("Die Erweiterung {0} stammt nicht vom FPLedit-Entwickler. Sie sollten die Erweiterung nur aktivieren, wenn Sie " +
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
            public static readonly string Activated = LocalizationHelper._("Aktiviert");
            public static readonly string Deactivated = LocalizationHelper._("Deaktiviert");
            public static readonly string RestartInfo = LocalizationHelper._("Die Änderungen treten erst nach dem nächsten Programmstart in Kraft.");
            public static readonly string RestartButton = LocalizationHelper._("Jetzt neu starten");
        }
    }
}
