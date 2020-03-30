using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.TimetableEditor
{
    internal sealed class ShuntForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button removeButton;
        private readonly Label arrivalLabel, departureLabel;
#pragma warning restore CS0649

        private readonly ArrDep arrDep;
        private readonly Station station;

        private readonly IEnumerable<ShuntMove> shuntBackup;

        public ShuntForm(ArrDep arrDep, Station sta)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.arrDep = arrDep;
            this.station = sta;

            arrivalLabel.Font = new Font(arrivalLabel.Font.FamilyName, arrivalLabel.Font.Size, FontStyle.Bold);
            departureLabel.Font = new Font(departureLabel.Font.FamilyName, departureLabel.Font.Size, FontStyle.Bold);
            
            arrivalLabel.Text = arrivalLabel.Text.Replace("{time}", arrDep.Arrival != default ? arrDep.Arrival.ToShortTimeString() : "-");
            arrivalLabel.Text = arrivalLabel.Text.Replace("{track}", arrDep.ArrivalTrack);
            
            departureLabel.Text = departureLabel.Text.Replace("{time}", arrDep.Departure != default ? arrDep.Departure.ToShortTimeString() : "-");
            departureLabel.Text = departureLabel.Text.Replace("{track}", arrDep.DepartureTrack);

            Title = Title.Replace("{station}", station.SName);

            var tracks = sta.Tracks.Select(t => t.Name);

            gridView.AddColumn<ShuntMove, TimeEntry>(s => s.Time, ts => ts.ToShortTimeString(), s => { TimeEntry.TryParse(s, out var res); return res; }, "Zeit", editable: true);
            gridView.AddDropDownColumn<ShuntMove>(s => s.SourceTrack, tracks, "Startgleis", editable: true);
            gridView.AddDropDownColumn<ShuntMove>(s => s.TargetTrack, tracks, "Zielgleis", editable: true);
            gridView.AddCheckColumn<ShuntMove>(s => s.EmptyAfterwards, "Alle Wagen?", editable: true);

            gridView.SelectedItemsChanged += (s, e) => removeButton.Enabled = gridView.SelectedItem != null;


            this.AddSizeStateHandler();

            shuntBackup = arrDep.ShuntMoves.Select(s => s.Clone<ShuntMove>()).ToList();

            RefreshList();
        }

        private void RefreshList()
        {
            gridView.DataStore = arrDep.ShuntMoves;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var shunt = new ShuntMove(station._parent);
            arrDep.ShuntMoves.Add(shunt);
            RefreshList();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedItem == null)
                return;

            arrDep.ShuntMoves.Remove((ShuntMove)gridView.SelectedItem);
            RefreshList();
        }

        private void SortButton_Click(object sender, EventArgs e)
        {
            arrDep.ShuntMoves.Sort(s => s.Time);
            RefreshList();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            // Automatisch sortieren
            arrDep.ShuntMoves.Sort(s => s.Time);
            RefreshList();

            var outOfRange = arrDep.ShuntMoves.Any(
                shunt => (shunt.Time < arrDep.Arrival && arrDep.Arrival != default) || (shunt.Time > arrDep.Departure && arrDep.Departure != default));
            if (outOfRange)
            {
                MessageBox.Show("Einige Rangierfahrten befinden sich außerhalb des Zeitfensters des Aufenthalts an der Station!", "FPLedit", MessageBoxType.Error);
                return;
            }
            
            if (!string.IsNullOrEmpty(arrDep.DepartureTrack) && arrDep.ShuntMoves.Last().TargetTrack != arrDep.DepartureTrack)
            {
                var res = MessageBox.Show("Die letzte Rangierfahrt endet nicht am Abfahrtsgleis! Trotzdem fortfahren?", "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                if (res == DialogResult.No) return;
            }
            if (!string.IsNullOrEmpty(arrDep.ArrivalTrack) && arrDep.ShuntMoves.First().SourceTrack != arrDep.ArrivalTrack)
            {
                var res = MessageBox.Show("Die erste Rangierfahrt beginnt nicht am Ankunftsgleis! Trotzdem fortfahren?", "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                if (res == DialogResult.No) return;
            }
            
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            arrDep.ShuntMoves.Clear();
            foreach (var shunt in shuntBackup)
                arrDep.ShuntMoves.Add(shunt);

            Close(DialogResult.Cancel);
        }
    }
}
