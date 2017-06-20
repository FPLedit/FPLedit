using FPLedit.BuchfahrplanExport.Model;
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

namespace FPLedit.BuchfahrplanExport
{
    public partial class VelocityForm : Form
    {
        private IInfo info;
        private Timetable tt;
        private BfplAttrs attrs;

        private string defaultVelocity = "";

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

            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");
            if (attrsEn != null)
                attrs = new BfplAttrs(attrsEn, tt);

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
            {
                listView.Items.Add(new ListViewItem(new[] {
                    p.Kilometre.ToString(),
                    p.SName,
                    p.GetAttribute("fpl-vmax", defaultVelocity),
                    p.Wellenlinien.ToString(),
                })
                { Tag = p });
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddPoint()
        {
            VelocityEditForm vef = new VelocityEditForm(tt);
            if (vef.ShowDialog() == DialogResult.OK)
            {
                var point = (BfplPoint)vef.Station;
                if (attrs != null)
                    attrs.AddPoint(point);
                UpdateStations();
            }
        }

        private void EditVmax(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];

                var sta = (IStation)item.Tag;

                VelocityEditForm vef = new VelocityEditForm(sta);
                if (vef.ShowDialog() == DialogResult.OK)
                {
                    UpdateStations();
                    item.Selected = true;
                    item.EnsureVisible();
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
                    UpdateStations();
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
            => EditVmax();

        private void trainListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditVmax(false);

        private void addButton_Click(object sender, EventArgs e)
            => AddPoint();

        private void deleteButton_Click(object sender, EventArgs e)
            => RemovePoint();

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
            => SelectPoint();
        #endregion
    }
}
