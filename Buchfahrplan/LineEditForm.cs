using Buchfahrplan.FileModel;
using Microsoft.VisualBasic;
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

        private void changeNameButton_Click(object sender, EventArgs e)
        {
            string newName = Interaction.InputBox("Bitte einen neuen Namen eingeben:", "Namen ändern");

            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];

                stations[stations.IndexOf((Station)item.Tag)].Name = newName;

                UpdateStations();
            }
        }

        private void changePositionButton_Click(object sender, EventArgs e)
        {
            string newPosString = Interaction.InputBox("Bitte die neue Position eingeben:", "Position ändern");

            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];

                try
                {
                    stations[stations.IndexOf((Station)item.Tag)].Kilometre = Convert.ToSingle(newPosString);
                }
                catch
                {
                    MessageBox.Show("Fehler die eingegebene Zeichenfolge ist kein valider Wert für eine Kommazahl!");
                }

                UpdateStations();
            }
        }

        private void deleteStationButton_Click(object sender, EventArgs e)
        {
            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];
                stations.Remove((Station)item.Tag);

                UpdateStations();
            }
        }

        private void newStationButton_Click(object sender, EventArgs e)
        {
            string name = Interaction.InputBox("Bitte den Namen der Station eingeben:", "Station hinzufügen");
            float pos = 0f;
            int velocity = 0;
            try
            {
                pos = Convert.ToSingle(Interaction.InputBox("Bitte die Position eingeben:", "Station hinzufügen"));
            }
            catch
            {
                MessageBox.Show("Fehler die eingegebene Zeichenfolge ist kein valider Wert für eine Kommazahl!");
                return;
            }

            try
            {
                velocity = Convert.ToInt32(Interaction.InputBox("Bitte die Höchsgeschwindigkeit eingeben:", "Station hinzufügen"));
            }
            catch
            {
                MessageBox.Show("Fehler die eingegebene Zeichenfolge ist kein valider Wert für eine Ganzzahl!");
                return;
            }

            Station sta = new Station()
            {
                Name = name,
                Kilometre = pos,
                MaxVelocity = velocity
            };

            stations.Add(sta);

            foreach (var t in trains)
            {
                t.Arrivals.Add(sta, new DateTime());
                t.Departures.Add(sta, new DateTime());
            }

            UpdateStations();
        }

        private void LineEditForm_Load(object sender, EventArgs e)
        {

        }

        private void changeVelocityButton_Click(object sender, EventArgs e)
        {
            string newVelString = Interaction.InputBox("Bitte die neue Höchsgeschwindigkeit eingeben:", "Höchsgeschwindigkeit ändern");

            if (stationListView.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)stationListView.Items[stationListView.SelectedIndices[0]];

                try
                {
                    stations[stations.IndexOf((Station)item.Tag)].MaxVelocity = Convert.ToInt32(newVelString);
                }
                catch
                {
                    MessageBox.Show("Fehler die eingegebene Zeichenfolge ist kein valider Wert für eine Ganzzahl!");
                }

                UpdateStations();
            }
        }
    }
}
