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
        private bool modified = false;
        private IRestartable restartable;

        public ExtensionsForm()
        {
            InitializeComponent();
        }

        public ExtensionsForm(ExtensionManager mg, IRestartable restartable) : this()
        {
            manager = mg;
            this.restartable = restartable;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (modified)
            {
                string newActivated = string.Join(";", enabledListView.Items.Cast<ListViewItem>().Select(i => (string)i.Tag).ToArray());
                SettingsManager.Set("extmgr.enabled", newActivated);

                var res = MessageBox.Show("Die Änderungen treten erst nach dem nächsten Programmstart in Kraft. Möchten sie das Programm jetzt neu starten?", "FPLedit", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                    restartable.RestartWithCurrentFile();
            }
            Close();
        }

        private void ExtensionsForm_Load(object sender, EventArgs e)
        {
            foreach (var ext in manager.EnabledPlugins)
            {
                enabledListView.Items.Add(new ListViewItem(ext.Name)
                    { Tag = ext.Plugin.GetType().FullName });
            }
            enabledListView.GotFocus += (s, a) =>
            {
                deactivateButton.Enabled = true;
                activateButton.Enabled = false;
            };

            foreach (var ext in manager.DisabledPlugins)
            {
                disabledListView.Items.Add(new ListViewItem(ext.Name)
                    { Tag = ext.Plugin.GetType().FullName });
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
                modified = true;
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
                modified = true;
            }
        }
    }
}
