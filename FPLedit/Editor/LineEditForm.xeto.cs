using Eto.Forms;
using FPLedit.Editor.Network;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class LineEditForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;
        private int route;

#pragma warning disable CS0649
        private GridView gridView;
        private Button loadLineButton;
#pragma warning restore CS0649

        private List<Station> stations;

        public LineEditForm(IInfo info, int route)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            tt = info.Timetable;
            this.route = route;

            info.BackupTimetable();

            if (tt.Type == TimetableType.Network)
                loadLineButton.Visible = false;

            gridView.AddColumn<Station>(s => s.SName, "Bahnhof");
            gridView.AddColumn<Station>(s => s.Positions.GetPosition(route).ToString(), "Position");

            gridView.MouseDoubleClick += (s, e) => EditStation(false);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;
            else
                gridView.KeyDown += HandleKeystroke;

            UpdateStations();

            this.AddCloseHandler();
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
                DeleteStation(false);
            else if (e.Key == Keys.L && e.Control)
                LoadLine();
            else if (e.Key == Keys.B && e.Control)
                EditStation(false);
            else if (e.Key == Keys.N && e.Control)
                NewStation();
        }

        private void UpdateStations()
        {
            stations = tt.GetRoute(route).GetOrderedStations();

            gridView.DataStore = stations;

            loadLineButton.Enabled = tt.Stations.Count == 0;
        }

        private void EditStation(bool message = true)
        {
            if (gridView.SelectedItems.Count() > 0)
            {
                Station station = (Station)gridView.SelectedItem;

                EditStationForm nsf = new EditStationForm(station, route);
                nsf.ShowModal(this);

                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station bearbeiten");
        }

        private void DeleteStation(bool message = true)
        {
            if (gridView.SelectedItems.Count() > 0)
            {
                Station sta = (Station)gridView.SelectedItem;
                tt.RemoveStation(sta);

                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station löschen");
        }

        private void NewStation()
        {
            EditStationForm nsf = new EditStationForm(tt, route);
            if (nsf.ShowModal(this) == DialogResult.Ok)
            {
                Station sta = nsf.Station;

                if (info.Timetable.Type == TimetableType.Network)
                {
                    var handler = new StaPosHandler();
                    handler.SetMiddlePos(route, sta, info.Timetable);
                    var r = sta.Routes.ToList();
                    r.Add(route);
                    sta.Routes = r.ToArray();
                }

                tt.AddStation(sta, route);
                UpdateStations();
            }
        }

        private void LoadLine()
        {
            if (tt.Stations.Count != 0)
                return;
            if (info.Timetable.Type == TimetableType.Network)
                throw new Exception("Streckendateien können bei Netzwerk-Fahrplänen nicht geladen werden!");

            IImport timport = new XMLImport();
            IImport simport = new XMLStationsImport();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddLegacyFilter(timport.Filter, simport.Filter);

            if (ofd.ShowDialog(this) == DialogResult.Ok)
            {
                IImport import = Path.GetExtension(ofd.FileName) == ".fpl" ? timport : simport;
                var ntt = import.Import(ofd.FileName, info.Logger);
                foreach (var station in ntt.Stations)
                    tt.AddStation(station, Timetable.LINEAR_ROUTE_ID);
                // ntt will be destroyed by decoupling stations, do not use afterwards!
            }

            UpdateStations();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            Result = DialogResult.Ok;
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();

            this.NClose();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditStation();

        private void deleteButton_Click(object sender, EventArgs e)
            => DeleteStation();

        private void newButton_Click(object sender, EventArgs e)
            => NewStation();

        private void loadLineButton_Click(object sender, EventArgs e)
            => LoadLine();
    }
}
