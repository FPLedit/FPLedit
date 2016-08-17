using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
{
    public partial class LineEditForm : Form
    {
        private IInfo info;
        private Timetable tt;        

        public LineEditForm()
        {
            InitializeComponent();

            stationListView.Columns.Add("Bahnhof");
            stationListView.Columns.Add("Position");
            stationListView.Columns.Add("Höchstgeschwindigkeit");
        }

        public void Init(IInfo info)
        {
            this.info = info;
            this.tt = info.Timetable;
            info.BackupTimetable();

            UpdateStations();
        }

        private void UpdateStations()
        {            
            stationListView.Items.Clear();

            foreach (var station in tt.Stations.OrderBy(s => s.Kilometre))
            {
                stationListView.Items.Add(new ListViewItem(new[] { 
                    station.Name,                     
                    station.Kilometre.ToString(),
                    station.MaxVelocity.ToString() })
                    { Tag = station });
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            info.RestoreTimetable();

            this.Close();
        }

        private void editStationButton_Click(object sender, EventArgs e)
        {
            if (stationListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station bearbeiten");
                return;
            }

            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];
                Station oldStation = tt.Stations[tt.Stations.IndexOf((Station)item.Tag)];

                NewStationForm nsf = new NewStationForm();
                nsf.Initialize(oldStation);
                DialogResult res = nsf.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                    UpdateStations();
            }
        }

        private void deleteStationButton_Click(object sender, EventArgs e)
        {
            if (stationListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Zuerst muss ien Zug ausgewählt werden!", "Zug löschen");
                return;
            }

            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];
                tt.Stations.Remove((Station)item.Tag);

                UpdateStations();
            }
        }

        private void newStationButton_Click(object sender, EventArgs e)
        {
            NewStationForm nsf = new NewStationForm();
            DialogResult res = nsf.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                Station sta = nsf.NewStation;

                tt.Stations.Add(sta);

                foreach (var t in tt.Trains)
                {
                    t.Arrivals.Add(sta, new TimeSpan());
                    t.Departures.Add(sta, new TimeSpan());
                }

                UpdateStations();
            }
        }
    }
}
