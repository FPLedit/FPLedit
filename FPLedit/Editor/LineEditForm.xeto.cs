using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Shared.Rendering;

namespace FPLedit.Editor;

internal sealed class LineEditForm : FDialog<DialogResult>
{
    private readonly IPluginInterface pluginInterface;
    private readonly int route;
    private object backupHandle = null!;

#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = default!;
#pragma warning restore CS0649,CA2213

    private Station[]? stations;

    public LineEditForm(IPluginInterface pluginInterface, int route)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        this.pluginInterface = pluginInterface;
        this.route = route;

        gridView.AddFuncColumn<Station>(s => s.SName, T._("Bahnhof"));
        gridView.AddFuncColumn<Station>(s => s.Positions.GetPosition(route).ToString()!, T._("abs. Kilometr."));
        gridView.AddFuncColumn<Station>(s => s.StationCode, T._("Abk."));
        gridView.AddFuncColumn<Station>(s => s.StationType, T._("Typ"));
        gridView.AddFuncColumn<Station>(s => s.Tracks.Count.ToString(), T._("Anzahl Gleise"));
        gridView.AddCheckColumn<Station>(s => s.RequestStop, T._("Bedarfshalt"));

        gridView.MouseDoubleClick += (_, _) => EditStation(false);

        if (Eto.Platform.Instance.IsWpf)
            KeyDown += HandleKeystroke;
        else
            gridView.KeyDown += HandleKeystroke;

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) gridView.AllowEmptySelection = false;

        pluginInterface.FileStateChanged += OnFileStateChanged;
        Closing += (_, _) => pluginInterface.FileStateChanged -= OnFileStateChanged;

        InitializeGrid();

        this.AddCloseHandler();
        this.AddSizeStateHandler();
    }

    private void OnFileStateChanged(object? s, FileStateChangedEventArgs e)
    {
        if (!Visible || IsDisposed) return;

        pluginInterface.ClearBackup(backupHandle);
        backupHandle = null!;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        backupHandle = pluginInterface.BackupTimetable();
        UpdateStations();
    }

    private void HandleKeystroke(object? sender, KeyEventArgs e)
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
            MessageBox.Show(T._("Zuerst muss eine Station ausgewählt werden!"), T._("Station bearbeiten"));
    }

    private void DeleteStation(bool message = true)
    {
        if (gridView.SelectedItems.Any())
        {
            var sta = (Station)gridView.SelectedItem;
            if (pluginInterface.Timetable.WouldProduceAmbiguousRoute(sta))
            {
                MessageBox.Show(T._("Sie versuchen eine Station zu löschen, ohne die danach zwei Routen zusammenfallen, das heißt zwei Stationen auf mehr als einer Route ohne Zwischenstation verbunden sind.\n\n" +
                                    "Der Konflikt kann nicht automatisch aufgehoben werden."), "FPLedit", MessageBoxType.Error);
                return;
            }
            if (sta.IsJunction)
            {
                MessageBox.Show(T._("Sie versuchen eine Station zu löschen, die an einem Kreuzungspunkt zweier Strecken liegt. Dies ist leider nicht möglich."), "FPLedit", MessageBoxType.Error);
                return;
            }

            if (MessageBox.Show(T._("Station wirklich löschen?"), "FPLedit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                pluginInterface.Timetable.RemoveStation(sta);
                UpdateStations();
            }
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss eine Station ausgewählt werden!"), T._("Station löschen"));
    }

    private void NewStation()
    {
        using var nsf = new EditStationForm(pluginInterface, pluginInterface.Timetable, route);
        var result = nsf.ShowModal(this);
        if (result == null) return;

        if (pluginInterface.Timetable.Type == TimetableType.Network)
        {
            var handler = new StationCanvasPositionHandler();
            handler.SetMiddlePos(route, result.Station, pluginInterface.Timetable);
        }

        pluginInterface.Timetable.AddStation(result.Station, route);
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

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Strecke bearbeiten");
        public static readonly string New = T._("&Neue Station");
        public static readonly string Edit = T._("Station &bearbeiten");
        public static readonly string Delete = T._("Station &löschen");
    }
}