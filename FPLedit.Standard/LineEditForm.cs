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

namespace FPLedit.Standard
{
    public partial class LineEditForm : Form
    {
        private IInfo info;
        private Timetable tt;

        public LineEditForm()
        {
            InitializeComponent();

            listView.Columns.Add("Bahnhof");
            listView.Columns.Add("Position");
        }

        public LineEditForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            UpdateStations();
        }

        private void UpdateStations()
        {
            listView.Items.Clear();

            foreach (var station in tt.Stations.OrderBy(s => s.Kilometre))
            {
                listView.Items.Add(new ListViewItem(new[] {
                    station.Name,
                    station.Kilometre.ToString() })
                { Tag = station });
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void EditStation(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                Station station = tt.Stations[tt.Stations.IndexOf((Station)item.Tag)];

                EditStationForm nsf = new EditStationForm(station);
                if (nsf.ShowDialog() == DialogResult.OK)
                {
                    UpdateStations();
                    var changedItem = listView.Items.OfType<ListViewItem>().Where(i => i.Tag == station).First();
                    changedItem.Selected = true;
                    changedItem.EnsureVisible();
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station bearbeiten");
        }

        private void EditMeta()
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                Station station = tt.Stations[tt.Stations.IndexOf((Station)item.Tag)];

                AttributeEdit mef = new AttributeEdit(station);
                if (mef.ShowDialog() == DialogResult.OK)
                {
                    UpdateStations();
                    var changedItem = listView.Items.OfType<ListViewItem>().Where(i => i.Tag == station).First();
                    changedItem.Selected = true;
                    changedItem.EnsureVisible();
                }
            }
            else
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Stations-Metadaten bearbeiten");
        }

        private void DeleteStation()
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                tt.Stations.Remove((Station)item.Tag);

                UpdateStations();
            }
            else
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station löschen");
        }

        private void NewStation()
        {
            EditStationForm nsf = new EditStationForm();
            if (nsf.ShowDialog() == DialogResult.OK)
            {
                Station sta = nsf.Station;

                tt.Stations.Add(sta);

                foreach (var t in tt.Trains)
                {
                    //t.Arrivals.Add(sta, new TimeSpan());
                    //t.Departures.Add(sta, new TimeSpan());
                    t.ArrDeps.Add(sta, new ArrDep());
                }

                UpdateStations();
                var changedItem = listView.Items.OfType<ListViewItem>().Where(i => i.Tag == sta).First();
                changedItem.Selected = true;
                changedItem.EnsureVisible();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditStation();

        private void deleteButton_Click(object sender, EventArgs e)
            => DeleteStation();

        private void newButton_Click(object sender, EventArgs e)
            => NewStation();

        private void editButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                EditMeta();
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditStation(false);
    }
}
