using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class TrainSortDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown stationsComboBox;
        private readonly StackLayout sortSelectionStack;
#pragma warning restore CS0649
        private readonly SelectionUI sortSelection;

        private TrainDirection direction;
        private Timetable tt;

        private bool shown;

        public TrainSortDialog(TrainDirection dir, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            direction = dir;
            this.tt = tt;

            //TODO: Investigate & re-enable linear sorting top-to-bottom/bottom-to-top
            //sortSelection = new SelectionUI(SelectMode, sortSelectionStack, "Nach Namen", "Nach Zugnummern (Name ohne Zugart)", "Nach Zeit, an Station", "Von oben nach unten", "Von unten nach oben");
            sortSelection = new SelectionUI(SelectMode, sortSelectionStack, "Nach Namen", "Nach Zugnummern (Name ohne Zugart)", "Nach Zeit, an Station");

            stationsComboBox.ItemTextBinding = Binding.Property<Station, string>(s => s.SName);
            stationsComboBox.DataStore = tt.Stations;

            if (dir == TrainDirection.tr) // Netzwerk-Fahrplan
            {
                sortSelection.DisableOption(3);
                sortSelection.DisableOption(4);
            }

            Shown += (s, e) => shown = true;
        }

        private void SelectMode(int idx)
        {
            if (direction == TrainDirection.tr && shown && !sortSelection.EnabledOptionSelected)
                MessageBox.Show("Die gewählte Option ist im Netzwerk-Fahrplan nicht verfügbar.", "FPLedit");

            stationsComboBox.Enabled = idx == 2;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (direction == TrainDirection.tr && !sortSelection.EnabledOptionSelected)
            {
                MessageBox.Show("Die gewählte Option ist im Netzwerk-Fahrplan nicht verfügbar.", "FPLedit");
                return;
            }

            var th = new TrainEditHelper();

            switch(sortSelection.SelectedState)
            {
                case 0: th.SortTrainsName(tt, direction, false); break;
                case 1: th.SortTrainsName(tt, direction, true); break;
                case 2: th.SortTrainsAtStation(tt, direction, (Station)stationsComboBox.SelectedValue); break;
                case 3: th.SortTrainsAllStations(tt, direction, true); break;
                case 4: th.SortTrainsAllStations(tt, direction, false); break;
            }

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
