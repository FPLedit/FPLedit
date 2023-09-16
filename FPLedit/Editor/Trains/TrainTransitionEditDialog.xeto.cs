using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;

namespace FPLedit.Editor.Trains;

internal sealed class TrainTransitionEditDialog : FDialog<TransitionEntry?>
{
#pragma warning disable CS0649,CA2213
    private readonly DropDown nextTrainDropDown = default!, stationDropDown = default!;
    private readonly DaysControl daysControl = default!;
#pragma warning restore CS0649,CA2213

    /// <summary>
    /// Create a new dialog to edit (= discard + create new) the given transition entry.
    /// </summary>
    /// <param name="transition">Transition object that is used as blueprint, null if a new transition should be created. Note: The referenced object will not be mutated!</param>
    /// <param name="train">Train at which the transition should start.</param>
    /// <param name="tt">Current timetable instance.</param>
    public TrainTransitionEditDialog(TransitionEntry? transition, ITrain train, Timetable tt)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        var selectableTrains = tt.Trains.Where(t => t != train).OrderBy(t => t.TName).ToArray();
        var selectableStations = new Station?[] { null }.Union(train.GetPath()).ToArray();

        nextTrainDropDown.ItemTextBinding = Binding.Delegate<Train, string>(t => t.TName);
        nextTrainDropDown.DataStore = selectableTrains;

        //TODO: fix name for null display
        stationDropDown.ItemTextBinding = Binding.Delegate<Station?, string>(t => t?.SName ?? "<Alle Stationen>");
        stationDropDown.DataStore = selectableStations;

        //TODO: check if in selectableTrains/selectableStations
        if (transition != null && !selectableTrains.Contains(transition.NextTrain))
        {
            throw new NotImplementedException();
        }
        if (transition?.Station != null && !selectableStations.Contains(transition.Station))
        {
            throw new NotImplementedException();
        }

        nextTrainDropDown.SelectedValue = transition?.NextTrain;
        daysControl.SelectedDays = transition?.Days ?? Days.All;
        stationDropDown.SelectedValue = transition?.Station;
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (nextTrainDropDown.SelectedIndex == -1)
        {
            MessageBox.Show(T._("Bitte einen Folgezug auswählen!"), "FPLedit");
            Result = null;
            return;
        }

        var transition = new TransitionEntry((ITrain) nextTrainDropDown.SelectedValue, daysControl.SelectedDays, (Station?) stationDropDown.SelectedValue);

        Close(transition);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(null);

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Übernehmen");
        public static readonly string NextTrain = T._("Folgezug");
        public static readonly string Station = T._("Station (optional)");
        public static readonly string Days = T._("Tage, an denen der Folgezug übergeht");
        public static readonly string Title = T._("Folgezug bearbeiten");
    }
}