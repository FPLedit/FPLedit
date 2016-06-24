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
        private List<Station> stations;
        private List<Station> stations_undo;

        private List<Train> trains;
        private List<Train> trains_undo;

        public LineEditForm()
        {
            InitializeComponent();

            stationListView.Columns.Add("Bahnhof");
            stationListView.Columns.Add("Position");
            stationListView.Columns.Add("Höchstgeschwindigkeit");
        }

        public void Init(List<Station> stations, List<Train> trains)
        {
            this.stations = stations;
            this.stations_undo = stations;

            this.trains = trains;
            this.trains_undo = trains;

            UpdateStations();
        }

        private void UpdateStations()
        {            
            stationListView.Items.Clear();

            foreach (var station in stations.OrderBy(s => s.Kilometre))
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
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            stations = stations_undo;
            trains = trains_undo;

            this.Close();
        }

        private void lineEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
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
                Station oldStation = stations[stations.IndexOf((Station)item.Tag)];

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
                stations.Remove((Station)item.Tag);

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

                stations.Add(sta);

                foreach (var t in trains)
                {
                    t.Arrivals.Add(sta, new TimeSpan());
                    t.Departures.Add(sta, new TimeSpan());
                }

                UpdateStations();
            }
        }
    }
}
