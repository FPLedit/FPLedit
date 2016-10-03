using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public partial class MetaEdit : Form
    {
        private Meta entity;
        private Dictionary<string, string> metaBackup;

        public MetaEdit()
        {
            InitializeComponent();

            listView1.Columns.Add("Key");
            listView1.Columns.Add("Value");
        }

        public void Initialize(Meta meta)
        {
            entity = meta;
            metaBackup = new Dictionary<string, string>(meta.Metadata);

            Text = "Metadaten-Editor: " + meta.GetType().Name + "/" + meta.ToString();

            UpdateView();
        }

        private void UpdateView()
        {
            listView1.Items.Clear();

            foreach (var entry in entity.Metadata)
            {
                listView1.Items.Add(new ListViewItem(new[]
                {
                    entry.Key,
                    entry.Value
                })
                {
                    Tag = entry.Key
                });
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            entity.Metadata = metaBackup;
            Close();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            MetaEditForm mef = new MetaEditForm();
            if (mef.ShowDialog() == DialogResult.OK)
            {
                entity.Metadata[mef.Meta.Key] = mef.Meta.Value;
                UpdateView();
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss eine Eigenschaft ausgewählt werden!", "Eigenschaft bearbeiten");
                return;
            }

            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.Items[listView1.SelectedIndices[0]];
                string key = (string)item.Tag;

                MetaEditForm mef = new MetaEditForm();
                mef.Initialize(new KeyValuePair<string, string>(key, entity.Metadata[key]));
                if (mef.ShowDialog() == DialogResult.OK)
                {
                    entity.Metadata[mef.Meta.Key] = mef.Meta.Value;
                    UpdateView();
                }                    
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss eine Eigenschaft ausgewählt werden!", "Eigenschaft löschen");
                return;
            }

            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.Items[listView1.SelectedIndices[0]];
                entity.Metadata.Remove((string)item.Tag);

                UpdateView();
            }
        }
    }
}
