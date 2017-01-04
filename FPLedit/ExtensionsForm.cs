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
        ExtensionManager manager;        

        public ExtensionsForm()
        {
            InitializeComponent();
        }

        public ExtensionsForm(ExtensionManager mg) : this()
        {
            manager = mg;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string newActivated = string.Join(";", enabledListView.Items.Cast<ListViewItem>().Select(i => (string)i.Tag).ToArray());
            SettingsManager.Set("EnabledExtensions", newActivated);
            Close();
        }

        private void ExtensionsForm_Load(object sender, EventArgs e)
        {
            foreach (var ext in manager.EnabledPlugins)
            {
                enabledListView.Items.Add(new ListViewItem(ext.Name)
                {
                    Tag = ext.Plugin.GetType().FullName,
                });
            }

            foreach (var ext in manager.DisabledPlugins)
                disabledListView.Items.Add(new ListViewItem(ext.Name)
                {
                    Tag = ext.Plugin.GetType().FullName,
                });
        }

        private void deactivateButton_Click(object sender, EventArgs e)
        {
            if (enabledListView.SelectedItems.Count > 0)
            {
                var item = enabledListView.SelectedItems[0];
                enabledListView.Items.Remove(item);
                disabledListView.Items.Add(item);
                disabledListView.SelectedIndices.Clear();
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
            }
        }
    }
}
