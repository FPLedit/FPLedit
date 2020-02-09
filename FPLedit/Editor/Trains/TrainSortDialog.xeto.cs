using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainSortDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown stationsComboBox;
        private readonly StackLayout sortSelectionStack;
#pragma warning restore CS0649
        private readonly SelectionUI<SortSelectionType> sortSelection;

        private readonly TrainDirection direction;
        private readonly Timetable tt;

        public TrainSortDialog(TrainDirection dir, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            direction = dir;
            this.tt = tt;

            sortSelection = new SelectionUI<SortSelectionType>(SelectMode, sortSelectionStack);

            stationsComboBox.ItemTextBinding = Binding.Property<Station, string>(s => s.SName);
            stationsComboBox.DataStore = tt.Stations;

            //TODO: Investigate & re-enable linear sorting top-to-bottom/bottom-to-top
            //if (dir == TrainDirection.tr) // Netzwerk-Fahrplan
            //{
            //    // deaktiviert "von unten nach oben", "von oben nach unten"
            //    sortSelection.DisableOption(SortSelectionType.TimeDown);
            //    sortSelection.DisableOption(SortSelectionType.TimeUp);
            //}
        }

        private void SelectMode(SortSelectionType option)
        {
            if (direction == TrainDirection.tr && WindowShown && !sortSelection.EnabledOptionSelected)
                MessageBox.Show("Die gewählte Option ist im Netzwerk-Fahrplan nicht verfügbar.", "FPLedit");

            stationsComboBox.Enabled = option == SortSelectionType.TimeStation;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (direction == TrainDirection.tr && !sortSelection.EnabledOptionSelected)
            {
                MessageBox.Show("Die gewählte Option ist im Netzwerk-Fahrplan nicht verfügbar.", "FPLedit");
                return;
            }

            var th = new TrainEditHelper();

            switch (sortSelection.SelectedState)
            {
                case SortSelectionType.Name:        th.SortTrainsName(tt, direction, false); break;
                case SortSelectionType.TrainNumber: th.SortTrainsName(tt, direction, true); break;
                case SortSelectionType.TimeStation: th.SortTrainsAtStation(tt, direction, (Station)stationsComboBox.SelectedValue); break;
                //case SortSelectionType.TimeDown:    th.SortTrainsAllStations(tt, direction, true); break;
                //case SortSelectionType.TimeUp:      th.SortTrainsAllStations(tt, direction, false); break;
            }

            Close(DialogResult.Ok);
        }

        protected override void Dispose(bool disposing)
        {
            sortSelection?.Dispose();
            base.Dispose(disposing);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        private enum SortSelectionType
        {
            [SelectionName("Nach Namen")]
            Name,
            [SelectionName("Nach Zugnummern (Name ohne Zugart)")]
            TrainNumber,
            [SelectionName("Nach Zeit, an Station")]
            TimeStation,
            //TODO: Investigate & re-enable linear sorting top-to-bottom/bottom-to-top
            //[SelectionName("Von oben nach unten")]
            //TimeDown,
            //[SelectionName("Von unten nach oben")]
            //TimeUp,
        }
    }
}
