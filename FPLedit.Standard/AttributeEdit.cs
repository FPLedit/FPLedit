using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class AttributeEdit : Form
    {
        private Entity entity;
        private Dictionary<string, string> attrBackup;

        public AttributeEdit()
        {
            InitializeComponent();

            listView.Columns.Add("Key");
            listView.Columns.Add("Value");
        }

        public AttributeEdit(Entity ent) : this()
        {
            entity = ent;
            attrBackup = new Dictionary<string, string>(ent.Attributes);

            Text = "Attribut-Editor: " + ent.GetType().Name + "/" + ent.ToString();

            UpdateView();
        }

        private void UpdateView()
        {
            listView.Items.Clear();

            foreach (var entry in entity.Attributes)
            {
                listView.Items.Add(new ListViewItem(new[]
                {
                    entry.Key,
                    entry.Value
                })
                { Tag = entry.Key });
            }
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }        

        private void NewMeta()
        {
            AttributeEditForm mef = new AttributeEditForm();
            if (mef.ShowDialog() == DialogResult.OK)
            {
                entity.Attributes[mef.Meta.Key] = mef.Meta.Value;
                UpdateView();
                var changedItem = listView.Items.OfType<ListViewItem>().Where(i => (string)i.Tag == mef.Meta.Key).First();
                changedItem.Selected = true;
                changedItem.EnsureVisible();
            }
        }

        private void EditMeta(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                string key = (string)item.Tag;

                AttributeEditForm mef = new AttributeEditForm(new KeyValuePair<string, string>(key, entity.Attributes[key]));
                if (mef.ShowDialog() == DialogResult.OK)
                {
                    entity.Attributes[mef.Meta.Key] = mef.Meta.Value;
                    UpdateView();
                    var changedItem = listView.Items.OfType<ListViewItem>().Where(i => (string)i.Tag == mef.Meta.Key).First();
                    changedItem.Selected = true;
                    changedItem.EnsureVisible();
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Eigenschaft ausgewählt werden!", "Eigenschaft bearbeiten");
        }

        private void DeleteMeta()
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                entity.Attributes.Remove((string)item.Tag);

                UpdateView();
            }
            else
                MessageBox.Show("Zuerst muss eine Eigenschaft ausgewählt werden!", "Eigenschaft löschen");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            entity.Attributes = attrBackup;
            Close();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditMeta();

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditMeta(false);

        private void newButton_Click(object sender, EventArgs e)
            => NewMeta();

        private void deleteButton_Click(object sender, EventArgs e)
            => DeleteMeta();
    }
}
