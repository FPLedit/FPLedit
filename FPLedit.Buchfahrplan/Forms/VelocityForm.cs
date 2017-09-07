﻿using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan
{
    public partial class VelocityForm : Form
    {
        private IInfo info;
        private Timetable tt;
        private BfplAttrs attrs;

        public VelocityForm()
        {
            InitializeComponent();

            listView.Columns.Add("km");
            listView.Columns.Add("Name");
            listView.Columns.Add("Vmax");
            listView.Columns.Add("Wellenlinien");
        }

        public VelocityForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;

            attrs = BfplAttrs.GetAttrs(tt);

            info.BackupTimetable();
            UpdateStations();
        }

        private void UpdateStations()
        {
            listView.Items.Clear();

            List<IStation> points = new List<IStation>();
            points.AddRange(tt.Stations);
            if (attrs != null)
                points.AddRange(attrs.Points);

            foreach (var p in points.OrderBy(o => o.Kilometre))
                AddPointToList(p);

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddPoint()
        {
            VelocityEditForm vef = new VelocityEditForm(tt);
            if (vef.ShowDialog() == DialogResult.OK)
            {
                var p = (BfplPoint)vef.Station;
                if (attrs != null)
                    attrs.AddPoint(p);
                AddPointToList(p);
            }
        }

        private void AddPointToList(IStation s)
        {
            listView.Items.Add(new ListViewItem(new[] {
                s.Kilometre.ToString(),
                s.SName,
                s.Vmax,
                s.Wellenlinien.ToString(),
            })
            { Tag = s });
        }

        private void EditPoint(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];

                var sta = (IStation)item.Tag;

                VelocityEditForm vef = new VelocityEditForm(sta);
                if (vef.ShowDialog() == DialogResult.OK)
                {
                    item.SubItems[0].Text = sta.Kilometre.ToString();
                    item.SubItems[1].Text = sta.SName;
                    item.SubItems[2].Text = sta.Vmax;
                    item.SubItems[3].Text = sta.Wellenlinien.ToString();

                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Zeile ausgewählt werden!", "Höchstgeschwindigkeit ändern");
        }

        private void RemovePoint(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];

                if (item.Tag.GetType() == typeof(Station))
                    throw new NotSupportedException("Bahnhöfe können nicht gelöscht werden!");
                else if (item.Tag.GetType() == typeof(BfplPoint))
                {
                    BfplPoint point = (BfplPoint)item.Tag;
                    if (attrs != null)
                        attrs.RemovePoint(point);

                    listView.Items.Remove(item);
                }

            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Zeile ausgewählt werden!", "Löschen");
        }

        private void SelectPoint()
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];

                deleteButton.Enabled = (item.Tag.GetType() == typeof(BfplPoint));
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();
            Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            info.ClearBackup();
            Close();
        }

        #region Events
        private void editButton_Click(object sender, EventArgs e)
            => EditPoint();

        private void trainListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditPoint(false);

        private void addButton_Click(object sender, EventArgs e)
            => AddPoint();

        private void deleteButton_Click(object sender, EventArgs e)
            => RemovePoint();

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
            => SelectPoint();
        #endregion
    }
}