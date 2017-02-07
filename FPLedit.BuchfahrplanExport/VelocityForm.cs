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
        private BFPL_Attrs attrs;

        private string defaultVelocity = "";

        public VelocityForm()
        {
            InitializeComponent();

            listView.Columns.Add("km");
            listView.Columns.Add("Name");
            listView.Columns.Add("Vmax");
        }

        public VelocityForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;

            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");
            if (attrsEn != null)
                attrs = new BFPL_Attrs(attrsEn, tt);

            info.BackupTimetable();
            UpdateStations();
        }

        private void UpdateStations()
        {
            listView.Items.Clear();

            var kms = new List<float>();
            if (attrs != null)
                kms = attrs.Points.Select(p => p.Kilometre).ToList();

            var skms = tt.Stations.Select(s => s.Kilometre).ToList();

            kms.AddRange(skms);
            var okms = kms.OrderBy(k => k);

            foreach (var km in okms)
            {
                bool stationExists = skms.Contains(km);
                if (stationExists)
                {
                    Station sta = tt.Stations.First(s => s.Kilometre == km);

                    listView.Items.Add(new ListViewItem(new[] {
                            sta.Kilometre.ToString(),
                            sta.SName,
                            sta.GetAttribute<string>("fpl-vmax", defaultVelocity),
                        })
                    { Tag = sta });
                }
                else if (attrs != null)
                {
                    BFPL_Point point = attrs.Points.First(p => p.Kilometre == km);
                    listView.Items.Add(new ListViewItem(new[] {
                            point.Kilometre.ToString(),
                            point.PName,
                            point.GetAttribute<string>("fpl-vmax", defaultVelocity),
                        })
                    { Tag = point });
                }
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddPoint()
        {
            VelocityEditForm vef = new VelocityEditForm(tt);
            if (vef.ShowDialog() == DialogResult.OK)
            {
                var point = vef.Point;
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

                if (item.Tag.GetType() == typeof(Station))
                {
                    Station station = (Station)item.Tag;

                    VelocityEditForm vef = new VelocityEditForm(station);
                    if (vef.ShowDialog() == DialogResult.OK)
                    {
                        UpdateStations();
                        item.Selected = true;
                        item.EnsureVisible();
                    }
                }
                else if (item.Tag.GetType() == typeof(BFPL_Point))
                {
                    BFPL_Point point = (BFPL_Point)item.Tag;
                    VelocityEditForm vef = new VelocityEditForm(point);
                    if (vef.ShowDialog() == DialogResult.OK)
                    {
                        UpdateStations();
                        item.Selected = true;
                        item.EnsureVisible();
                    }
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
                else if (item.Tag.GetType() == typeof(BFPL_Point))
                {
                    BFPL_Point point = (BFPL_Point)item.Tag;
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

                deleteButton.Enabled = (item.Tag.GetType() == typeof(BFPL_Point));
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

        #region events
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
