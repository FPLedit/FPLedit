using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    internal class ShuntForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button removeButton, upButton, downButton;
        private readonly Label arrivalLabel, departureLabel;
#pragma warning restore CS0649

        private ArrDep arrDep;
        private Station station;

        private IEnumerable<ShuntMove> shuntBackup;

        public ShuntForm(ArrDep arrDep, Station sta)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.arrDep = arrDep;
            this.station = sta;

            arrivalLabel.Font = new Font(arrivalLabel.Font.FamilyName, arrivalLabel.Font.Size, FontStyle.Bold);
            departureLabel.Font = new Font(departureLabel.Font.FamilyName, departureLabel.Font.Size, FontStyle.Bold);
            arrivalLabel.Text = arrivalLabel.Text.Replace("{time}", arrDep.Arrival != default ? arrDep.Arrival.ToShortTimeString() : "-");
            departureLabel.Text = departureLabel.Text.Replace("{time}", arrDep.Departure != default ? arrDep.Departure.ToShortTimeString() : "-");

            Title = Title.Replace("{station}", station.SName);

            var tracks = sta.Tracks.Select(t => t.Name);

            gridView.AddColumn<ShuntMove, TimeSpan>(s => s.Time, ts => ts.ToShortTimeString(), s => { TimeSpan.TryParse(s, out var res); return res; }, "Zeit", editable: true);
            gridView.AddDropDownColumn<ShuntMove>(s => s.SourceTrack, tracks, "Startgleis", editable: true);
            gridView.AddDropDownColumn<ShuntMove>(s => s.TargetTrack, tracks, "Zielgleis", editable: true);
            gridView.AddCheckColumn<ShuntMove>(s => s.EmptyAfterwards, "Alle Wagen?", editable: true);

            gridView.SelectedItemsChanged += (s, e) =>
            {
                var shunt = (ShuntMove)gridView.SelectedItem;
                removeButton.Enabled = upButton.Enabled = downButton.Enabled = shunt != null;
            };

            this.AddSizeStateHandler();

            shuntBackup = arrDep.ShuntMoves.Select(s => s.Clone<ShuntMove>()).ToList();

            RefreshList();
        }

        private void RefreshList()
        {
            gridView.DataStore = arrDep.ShuntMoves;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var shunt = new ShuntMove(station._parent);
            arrDep.ShuntMoves.Add(shunt);
            RefreshList();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedItem == null)
                return;

            arrDep.ShuntMoves.Remove((ShuntMove)gridView.SelectedItem);
            RefreshList();
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedItem == null)
                return;

            var idx = gridView.SelectedRow;
            if (idx == 0)
                return;
            arrDep.ShuntMoves.Move(idx, idx - 1);
            RefreshList();
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedItem == null)
                return;

            var idx = gridView.SelectedRow;
            if (idx == arrDep.ShuntMoves.Count - 1)
                return;
            arrDep.ShuntMoves.Move(idx, idx + 1);
            RefreshList();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var outOfRange = arrDep.ShuntMoves.Any(
                shunt => (shunt.Time < arrDep.Arrival && arrDep.Arrival != default) || (shunt.Time > arrDep.Departure && arrDep.Departure != default));
            if (outOfRange)
            {
                MessageBox.Show("Einige Rangierfahrten befinden sich außerhalb des Zeitfensters des Aufenthalts an der Station!", "FPLedit", MessageBoxType.Error);
                return;
            }
            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            arrDep.ShuntMoves.Clear();
            foreach (var shunt in shuntBackup)
                arrDep.ShuntMoves.Add(shunt);

            Close(DialogResult.Cancel);
        }
    }
}
