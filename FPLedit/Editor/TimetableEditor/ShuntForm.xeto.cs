using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Editor.TimetableEditor;

internal sealed class ShuntForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = default!;
    private readonly Button removeButton = default!;
    private readonly Label arrivalLabel = default!, departureLabel = default!;
#pragma warning restore CS0649,CA2213

    private readonly ArrDep arrDep;
    private readonly Station station;

    private readonly IEnumerable<ShuntMove> shuntBackup;

    public ShuntForm(ITrain train, ArrDep arrDep, Station sta)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        this.arrDep = arrDep;
        this.station = sta;
            
        var dir = new NetworkHelper(train.ParentTimetable).GetTrainDirectionAtStation(train, sta);
        var th = new TrackHelper();

        arrivalLabel.Font = new Font(arrivalLabel.Font.FamilyName, arrivalLabel.Font.Size, FontStyle.Bold);
        departureLabel.Font = new Font(departureLabel.Font.FamilyName, departureLabel.Font.Size, FontStyle.Bold);
            
        var arrivalTrack = dir.HasValue ? th.GetTrack(train, sta, dir.Value, arrDep, TrackQuery.Departure) : "-";
        arrivalLabel.Text = arrivalLabel.Text.Replace("{time}", arrDep.Arrival != default ? arrDep.Arrival.ToTimeString() : "-");
        arrivalLabel.Text = arrivalLabel.Text.Replace("{track}", arrivalTrack);
            
        var departureTrack = dir.HasValue ? th.GetTrack(train, sta, dir.Value, arrDep, TrackQuery.Departure) : "-";
        departureLabel.Text = departureLabel.Text.Replace("{time}", arrDep.Departure != default ? arrDep.Departure.ToTimeString() : "-");
        departureLabel.Text = departureLabel.Text.Replace("{track}", departureTrack);

        Title = Title.Replace("{station}", station.SName);

        var tracks = sta.Tracks.Select(t => t.Name).ToArray();

        gridView.AddColumn<ShuntMove, TimeEntry>(s => s.Time, ts => ts.ToTimeString(), s => { TimeEntry.TryParse(s, out var res); return res; }, T._("Zeit"), editable: true);
        gridView.AddDropDownColumn<ShuntMove>(s => s.SourceTrack, tracks, T._("Startgleis"), editable: true);
        gridView.AddDropDownColumn<ShuntMove>(s => s.TargetTrack, tracks, T._("Zielgleis"), editable: true);
        gridView.AddCheckColumn<ShuntMove>(s => s.EmptyAfterwards, T._("Alle Wagen?"), editable: true);

        gridView.SelectedItemsChanged += (_, _) => removeButton.Enabled = gridView.SelectedItem != null;

        this.AddSizeStateHandler();

        shuntBackup = arrDep.Copy().ShuntMoves.ToList();

        Shown += (_, _) => RefreshList();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Keys.Delete)
        {
            e.Handled = true;
            RemoveShunt();
        }
        if (e.Key == Keys.N && e.Control)
        {
            e.Handled = true;
            NewShunt();
        }
        base.OnKeyDown(e);
    }

    private void RefreshList()
    {
        if (gridView != null! && !gridView.IsDisposed && gridView.Visible)
            gridView.DataStore = arrDep.ShuntMoves.ToArray(); // Array is here to prevent unpredictable crashes with ObservableCollection as DataStore.
    }

    private void NewShunt()
    {
        var shunt = new ShuntMove(station.ParentTimetable);
        arrDep.ShuntMoves.Add(shunt);
        RefreshList();
    }

    private void RemoveShunt()
    {
        if (gridView.SelectedItem == null)
            return;

        arrDep.ShuntMoves.Remove((ShuntMove) gridView.SelectedItem);
        RefreshList();
    }

    private void SortShunts()
    {
        arrDep.ShuntMoves.Sort(s => s.Time);
        RefreshList();
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        // Automatisch sortieren
        arrDep.ShuntMoves.Sort(s => s.Time);
        RefreshList();

        if (!ShuntChecks()) return;

        Close(DialogResult.Ok);
    }

    private bool ShuntChecks()
    {
        var outOfRange = arrDep.ShuntMoves.Any(
            shunt => (shunt.Time < arrDep.Arrival && arrDep.Arrival != default) || (shunt.Time > arrDep.Departure && arrDep.Departure != default));
        if (outOfRange)
        {
            MessageBox.Show(T._("Einige Rangierfahrten befinden sich außerhalb des Zeitfensters des Aufenthalts an der Station!"), "FPLedit", MessageBoxType.Error);
            return false;
        }

        var lastShuntTarget = arrDep.ShuntMoves.LastOrDefault()?.TargetTrack;
        if (!string.IsNullOrEmpty(arrDep.DepartureTrack) && lastShuntTarget != null && lastShuntTarget != arrDep.DepartureTrack)
        {
            var res = MessageBox.Show(T._("Die letzte Rangierfahrt endet nicht am Abfahrtsgleis! Trotzdem fortfahren?"), "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (res == DialogResult.No) return false;
        }

        var firstShuntSource = arrDep.ShuntMoves.FirstOrDefault()?.SourceTrack;
        if (!string.IsNullOrEmpty(arrDep.ArrivalTrack) && firstShuntSource != null && firstShuntSource != arrDep.ArrivalTrack)
        {
            var res = MessageBox.Show(T._("Die erste Rangierfahrt beginnt nicht am Ankunftsgleis! Trotzdem fortfahren?"), "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (res == DialogResult.No) return false;
        }

        //TODO: More shunt checks

        return true;
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        gridView.DataStore = null; // Disconnect DataStore to prevent exceptions when modifying collection afterwards.
            
        arrDep.ShuntMoves.Clear();
        foreach (var shunt in shuntBackup)
            arrDep.ShuntMoves.Add(shunt);

        Close(DialogResult.Cancel);
    }
        
    private void AddButton_Click(object sender, EventArgs e) => NewShunt();
    private void RemoveButton_Click(object sender, EventArgs e) => RemoveShunt();
    private void SortButton_Click(object sender, EventArgs e) => SortShunts();
        
    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Rangierfahrten am Bahnhof {station}");
        public static readonly string New = T._("&Hinzufügen");
        public static readonly string Delete = T._("&Löschen");
        public static readonly string Sort = T._("S&ortieren");
        public static readonly string ArrivalLabel = T._("Ankunft: {time} / Gleis {track}");
        public static readonly string DepartureLabel = T._("Abfahrt: {time} / Gleis {track}");
    }
}