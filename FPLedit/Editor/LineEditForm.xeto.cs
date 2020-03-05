using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FPLedit.Shared.Rendering;

namespace FPLedit.Editor
{
    internal sealed class LineEditForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly int route;
        private readonly object backupHandle;

#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button loadLineButton;
#pragma warning restore CS0649

        private IList<Station> stations;

        public LineEditForm(IPluginInterface pluginInterface, int route)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            tt = pluginInterface.Timetable;
            this.route = route;

            backupHandle = pluginInterface.BackupTimetable();

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
            this.AddSizeStateHandler();
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
            stations = tt.GetRoute(route).Stations;

            gridView.DataStore = stations;

            loadLineButton.Enabled = tt.Stations.Count == 0;
        }

        private void EditStation(bool message = true)
        {
            if (gridView.SelectedItems.Any())
            {
                var station = (Station)gridView.SelectedItem;

                using (var nsf = new EditStationForm(pluginInterface, station, route))
                    nsf.ShowModal(this);

                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station bearbeiten");
        }

        private void DeleteStation(bool message = true)
        {
            if (gridView.SelectedItems.Any())
            {
                var sta = (Station)gridView.SelectedItem;
                if (tt.WouldProduceAmbiguousRoute(sta))
                {
                    MessageBox.Show("Sie versuchen eine Station zu löschen, ohne die danach zwei Routen zusammenfallen, das heißt zwei Stationen auf mehr als einer Route ohne Zwischenstation verbunden sind.\n\n" +
                                    "Der Konflikt kann nicht automatisch aufgehoben werden.", "FPLedit", MessageBoxType.Error);
                    return;
                }
                if (sta.IsJunction)
                {
                    MessageBox.Show("Sie versuchen eine Station zu löschen, die an einem Kreuzungspunkt zweier Strecken liegt. Dies ist leider nicht möglich.", "FPLedit", MessageBoxType.Error);
                    return;
                }

                tt.RemoveStation(sta);
                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station löschen");
        }

        private void NewStation()
        {
            using (var nsf = new EditStationForm(pluginInterface, tt, route))
            {
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    var sta = nsf.Station;

                    if (pluginInterface.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StationCanvasPositionHandler();
                        handler.SetMiddlePos(route, sta, pluginInterface.Timetable);
                    }

                    tt.AddStation(sta, route);
                    UpdateStations();
                }
            }
        }

        private void LoadLine()
        {
            if (tt.Stations.Count != 0)
                return;
            if (pluginInterface.Timetable.Type == TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "load line file");

            IImport timport = new XMLImport();
            IImport simport = new NonDefaultFiletypes.XmlStationsImport();

            using (var ofd = new OpenFileDialog())
            {
                ofd.AddLegacyFilter(timport.Filter, simport.Filter);

                if (ofd.ShowDialog(this) == DialogResult.Ok)
                {
                    IImport import = Path.GetExtension(ofd.FileName) == ".fpl" ? timport : simport;
                    var ntt = import.SafeImport(ofd.FileName, pluginInterface);
                    foreach (var station in ntt.Stations)
                        tt.AddStation(station, Timetable.LINEAR_ROUTE_ID);
                    // ntt will be destroyed by decoupling stations, do not use afterwards!
                }
            }

            UpdateStations();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            pluginInterface.ClearBackup(backupHandle);
            Result = DialogResult.Ok;
            this.NClose();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            pluginInterface.RestoreTimetable(backupHandle);

            this.NClose();
        }

        private void EditButton_Click(object sender, EventArgs e)
            => EditStation();

        private void DeleteButton_Click(object sender, EventArgs e)
            => DeleteStation();

        private void NewButton_Click(object sender, EventArgs e)
            => NewStation();

        private void LoadLineButton_Click(object sender, EventArgs e)
            => LoadLine();
    }
}
