﻿using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Shared.Rendering;

namespace FPLedit.Editor
{
    internal sealed class LineEditForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly int route;
        private object backupHandle;

#pragma warning disable CS0649
        private readonly GridView gridView;
#pragma warning restore CS0649

        private Station[] stations;

        public LineEditForm(IPluginInterface pluginInterface, int route)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            this.route = route;

            gridView.AddColumn<Station>(s => s.SName, "Bahnhof");
            gridView.AddColumn<Station>(s => s.Positions.GetPosition(route).ToString(), "abs. Kilometr.");
            gridView.AddColumn<Station>(s => s.StationCode, "Abk.");
            gridView.AddColumn<Station>(s => s.StationType, "Typ");
            gridView.AddColumn<Station>(s => s.Tracks.Count.ToString(), "Anzahl Gleise");
            gridView.AddCheckColumn<Station>(s => s.RequestStop, "Bedarfshalt");

            gridView.MouseDoubleClick += (s, e) => EditStation(false);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;
            else
                gridView.KeyDown += HandleKeystroke;

            pluginInterface.FileStateChanged += OnFileStateChanged;
            Closing += (s, e) => pluginInterface.FileStateChanged -= OnFileStateChanged;

            InitializeGrid();

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }
        
        private void OnFileStateChanged(object s, FileStateChangedEventArgs e)
        {
            if (!Visible || IsDisposed) return;

            pluginInterface.ClearBackup(backupHandle);
            backupHandle = null;
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            backupHandle = pluginInterface.BackupTimetable();
            UpdateStations();
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
                DeleteStation(false);
            else if (e.Key == Keys.B && e.Control)
                EditStation(false);
            else if (e.Key == Keys.N && e.Control)
                NewStation();
        }

        private void UpdateStations()
        {
            stations = pluginInterface.Timetable.GetRoute(route).Stations.ToArray();

            gridView.DataStore = stations;
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
                if (pluginInterface.Timetable.WouldProduceAmbiguousRoute(sta))
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

                pluginInterface.Timetable.RemoveStation(sta);
                UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Station löschen");
        }

        private void NewStation()
        {
            using (var nsf = new EditStationForm(pluginInterface, pluginInterface.Timetable, route))
            {
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    var sta = nsf.Station;

                    if (pluginInterface.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StationCanvasPositionHandler();
                        handler.SetMiddlePos(route, sta, pluginInterface.Timetable);
                    }

                    pluginInterface.Timetable.AddStation(sta, route);
                    UpdateStations();
                }
            }
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
    }
}
