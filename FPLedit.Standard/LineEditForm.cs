using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    DeleteStation(false);
                else if (e.KeyCode == Keys.L && e.Control)
                    LoadLine();
                else if (e.KeyCode == Keys.B && e.Control)
                    EditStation(false);
                else if (e.KeyCode == Keys.N && e.Control)
                    NewStation();
            };

            UpdateStations();
        }

        private void UpdateStations()
        {
            listView.Items.Clear();

            foreach (var station in tt.Stations.OrderBy(s => s.Kilometre))
            {
                listView.Items.Add(new ListViewItem(new[] {
                    station.SName,
                    station.Kilometre.ToString() })
                { Tag = station });
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            loadLineButton.Enabled = tt.Stations.Count == 0;
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

        private void DeleteStation(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                tt.RemoveStation((Station)item.Tag);

                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station löschen");
        }

        private void NewStation()
        {
            EditStationForm nsf = new EditStationForm(tt);
            if (nsf.ShowDialog() == DialogResult.OK)
            {
                Station sta = nsf.Station;

                tt.AddStation(sta);

                UpdateStations();
                var changedItem = listView.Items.OfType<ListViewItem>().Where(i => i.Tag == sta).First();
                changedItem.Selected = true;
                changedItem.EnsureVisible();
            }
        }

        private void LoadLine()
        {
            if (tt.Stations.Count != 0)
                return;

            IImport timport = new XMLImport();
            IImport simport = new XMLStationsImport();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = timport.Filter + "|" + simport.Filter;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                IImport import = Path.GetExtension(ofd.FileName) == ".fpl" ? timport : simport;
                var ntt = import.Import(ofd.FileName, info.Logger);
                foreach (var station in ntt.Stations)
                    tt.AddStation(station);
                ntt = null; // ntt will be destroyed by decoupling stations, do not use afterwards!
            }

            UpdateStations();
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

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditStation(false);

        private void loadLineButton_Click(object sender, EventArgs e)
            => LoadLine();
    }
}
