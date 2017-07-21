using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    public partial class ExtensionsForm : Form
    {
        private ExtensionManager manager;
        private IRestartable restartable;

        private static List<PluginInfo> enabledPlugins;

        public ExtensionsForm()
        {
            InitializeComponent();
        }

        internal ExtensionsForm(ExtensionManager mg, IRestartable restartable) : this()
        {
            manager = mg;
            this.restartable = restartable;
        }

        private void closeButton_Click(object sender, EventArgs e)
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

        private void ExtensionsForm_Load(object sender, EventArgs e)
        {
            var enabled_plgs = manager.Plugins.Where(p => p.Enabled);
            var disabled_plgs = manager.Plugins.Where(p => !p.Enabled);

            foreach (var plg in enabled_plgs)
            {
                enabledListView.Items.Add(new ListViewItem(plg.Name)
                    { Tag = plg });
            }
            enabledListView.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = true;
                activateButton.Enabled = false;
            };

            foreach (var plg in disabled_plgs)
            {
                disabledListView.Items.Add(new ListViewItem(plg.Name)
                    { Tag = plg });
            }
            disabledListView.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = false;
                activateButton.Enabled = true;
            };
        }

        private void deactivateButton_Click(object sender, EventArgs e)
        {
            if (enabledListView.SelectedItems.Count > 0)
            {
                var item = enabledListView.SelectedItems[0];
                enabledListView.Items.Remove(item);
                disabledListView.Items.Add(item);
                disabledListView.SelectedIndices.Clear();

                manager.Deactivate((PluginInfo)item.Tag);
            }
        }

        private void activateButton_Click(object sender, EventArgs e)
        {
            if (disabledListView.SelectedItems.Count > 0)
            {
                var item = disabledListView.SelectedItems[0];
                disabledListView.Items.Remove(item);
                enabledListView.Items.Add(item);
                enabledListView.SelectedIndices.Clear();

                manager.Activate((PluginInfo)item.Tag);
            }
        }
    }
}
