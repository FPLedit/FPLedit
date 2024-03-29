﻿using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.TrainLinks;

namespace FPLedit.Editor.Trains;

internal sealed class TrainCopyDialog : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox diffTextBox = default!, nameTextBox = default!, changeTextBox = default!, countTextBox = default!;
    private readonly CheckBox copyAllCheckBox = default!;
    private readonly StackLayout selectStack = default!, linkTypeStack = default!;
    private readonly TableLayout extendedOptionsTable = default!, copyOptionsTable = default!, autoNameOptionsTable = default!, specialNameOptionsTable = default!, moveOptionsTable = default!;
    private readonly Button closeButton = default!;
    private readonly GridView specialNameGridView = default!;
    private readonly DropDown moveStartStationDropDown = default!;
#pragma warning restore CS0649,CA2213
    private readonly NumberValidator diffValidator, countValidator, changeValidator;
    private readonly SelectionUI<CopySelectionMode> modeSelect;
    private readonly SelectionUI<LinkTypeMode> linkSelect;

    private readonly Train[] trains;
    private readonly Timetable tt;

    /// <summary>
    /// This temp station instance is used for marking the selection of the "respective first station".
    /// </summary>
    private readonly Station moveStartFirstStationStation;

    public TrainCopyDialog(Train[] t, Timetable tt)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        diffValidator = new NumberValidator(diffTextBox, false, true, errorMessage: T._("Bitte die Verschiebung als Zahl in Minuten angeben!"));
        countValidator = new NumberValidator(countTextBox, false, true, allowNegative: false, errorMessage: T._("Bitte eine gültige Anzahl >0 neuer Züge eingeben!"));
        changeValidator = new NumberValidator(changeTextBox, false, true, errorMessage: T._("Bitte eine gültige Veränderung der Zugnummer eingeben!"));

        trains = t;
        this.tt = tt;
        if (t.Length == 1)
            nameTextBox.Text = t.Single().TName;
        diffTextBox.Text = "+20";
        countTextBox.Text = "1";
        changeTextBox.Text = "2";

        modeSelect = new SelectionUI<CopySelectionMode>(_ => UpdateVisibility(), selectStack);
        linkSelect = new SelectionUI<LinkTypeMode>(_ => UpdateVisibility(), linkTypeStack);

        specialNameGridView.AddFuncColumn((SpecialNameEntry spn) => spn.RowNumber.ToString(), "");
        specialNameGridView.AddColumn((SpecialNameEntry spn) => spn.Name, T._("Zugname"), true);
        specialNameGridView.DataStore = new[] { new SpecialNameEntry(1, "") };

        moveStartFirstStationStation = new Station(tt) { SName = T._("<Erste Station>") };
        var selectableStations = new [] { moveStartFirstStationStation }.Union(t.SelectMany(x => x.GetPath()).Distinct()).ToArray();
        moveStartStationDropDown.ItemTextBinding = Binding.Delegate<Station, string>(s => s.SName);
        moveStartStationDropDown.DataStore = selectableStations;
        moveStartStationDropDown.SelectedValue = moveStartFirstStationStation;

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) specialNameGridView.AllowEmptySelection = false;

        if (t.Length > 1)
        {
            modeSelect.DisableOption(CopySelectionMode.Copy);
            modeSelect.DisableOption(CopySelectionMode.Link);
            modeSelect.ChangeSelection(CopySelectionMode.Move);
        }
    }

    private void CountTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!countValidator.Valid || specialNameGridView.DataStore == null)
            return;
        var count = int.Parse(countTextBox.Text);
        var oldStore = (SpecialNameEntry[])specialNameGridView.DataStore;
        IEnumerable<SpecialNameEntry> newStore;
        if (count <= oldStore.Length)
            newStore = oldStore.Take(count);
        else
        {
            var newList = new List<SpecialNameEntry>(oldStore);
            for (int i = oldStore.Length; i < count; i++)
                newList.Add(new SpecialNameEntry(i + 1, ""));
            newStore = newList;
        }

        specialNameGridView.DataStore = newStore.ToArray();
    }

    private void UpdateVisibility()
    {
        // Those two might be null when this is called when the constructor did not finish running...
        var mode = modeSelect?.SelectedState ?? CopySelectionMode.Copy;
        var linkType = linkSelect?.SelectedState ?? LinkTypeMode.Auto;

        extendedOptionsTable.Visible = copyOptionsTable.Visible = (mode == CopySelectionMode.Copy || mode == CopySelectionMode.Link);
        autoNameOptionsTable.Visible = (mode == CopySelectionMode.Copy || (mode == CopySelectionMode.Link && linkType == LinkTypeMode.Auto));
        moveOptionsTable.Visible = mode == CopySelectionMode.Move;
        specialNameOptionsTable.Visible = (mode == CopySelectionMode.Link && linkType == LinkTypeMode.Special);
        linkTypeStack.Visible = mode == CopySelectionMode.Link;
        copyAllCheckBox.Visible = mode == CopySelectionMode.Copy;

        closeButton.Text = mode switch
        {
            CopySelectionMode.Copy => T._("Kopieren"),
            CopySelectionMode.Move => T._("Verschieben"),
            CopySelectionMode.Link => T._("Verlinken"),
            _ => T._("Kopieren"),
        };
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        var copy = modeSelect.SelectedState == CopySelectionMode.Copy || modeSelect.SelectedState == CopySelectionMode.Link;

        if (!diffValidator.Valid || (copy && (!countValidator.Valid || !changeValidator.Valid)))
        {
            var msg = diffValidator.Valid ? "" : diffValidator.ErrorMessage + Environment.NewLine;
            if (copy)
            {
                msg += countValidator.Valid ? "" : countValidator.ErrorMessage + Environment.NewLine;
                msg += changeValidator.Valid ? "" : changeValidator.ErrorMessage + Environment.NewLine;
            }
            MessageBox.Show(T._("Bitte erst alle Felder korrekt ausfüllen:\n{0}", msg));
            Result = DialogResult.None;
            return;
        }

        var th = new TrainEditHelper();
        var diff = int.Parse(diffTextBox.Text);

        if (modeSelect.SelectedState == CopySelectionMode.Copy)
        {
            var count = int.Parse(countTextBox.Text);
            var add = int.Parse(changeTextBox.Text);

            var newTrains = th.CopyTrainMultiple(trains.Single(), diff, nameTextBox.Text, copyAllCheckBox.Checked!.Value, count, add);

            foreach (var newTrain in newTrains)
            {
                if (tt.Trains.Any(t => t.TName == newTrain.TName))
                {
                    if (MessageBox.Show(T._("Es ist bereits ein Zug mit der Zugnummer {0} in diesem Fahrplan vorhanden, soll diese Kopie trotzdem angelegt werden?", newTrain.TName),
                            T._("Züge kopieren"), MessageBoxButtons.YesNo) == DialogResult.No)
                        continue;
                }

                tt.AddTrain(newTrain);
            }
        }
        else if (modeSelect.SelectedState == CopySelectionMode.Link)
        {
            var count = int.Parse(countTextBox.Text);
            ITrainNameGen tnc;

            switch (linkSelect.SelectedState)
            {
                case LinkTypeMode.Auto:
                    var add = int.Parse(changeTextBox.Text);
                    tnc = new AutoTrainNameGen(nameTextBox.Text, add);
                    break;
                case LinkTypeMode.Special:
                    var entries = ((SpecialNameEntry[]) specialNameGridView.DataStore).Select(en => en.Name).ToArray();
                    if (entries.Any(string.IsNullOrEmpty))
                    {
                        MessageBox.Show(T._("Es wurden keinen Namen für alle Züge angegeben!"));
                        return;
                    }
                    tnc = new SpecialTrainNameGen(entries);
                    break;
                default:
                    throw new NotSupportedException("The selected LinkTypeMode is not defined!");
            }

            th.LinkTrainMultiple(trains.Single(), TimeEntry.Zero, new TimeEntry(0, diff), count, tnc);
        }
        else
        {
            var startStation = moveStartStationDropDown.SelectedValue == moveStartFirstStationStation ? null : (Station) moveStartStationDropDown.SelectedValue;
            th.MoveTrain(trains, diff, startStation);
        }

        Close(DialogResult.Ok);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(DialogResult.Cancel);

    protected override void Dispose(bool disposing)
    {
        modeSelect.Dispose();
        linkSelect.Dispose();
        base.Dispose(disposing);
    }

    private enum CopySelectionMode
    {
        [SelectionName("Zug kopieren")]
        Copy,
        [SelectionName("Zug verschieben")]
        Move,
        [SelectionName("Zug verlinken")]
        Link
    }

    private enum LinkTypeMode
    {
        [SelectionName("Automatische Benennung")]
        Auto,
        [SelectionName("Manuelle Benennung")]
        Special
    }

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Kopieren");
        public static readonly string Difference = T._("Taktverschiebung in Minuten");
        public static readonly string Count = T._("Anzahl der neuen Züge");
        public static readonly string BaseName = T._("Basiszugnummer");
        public static readonly string NumberChange = T._("Änderung der Zugnummer");
        public static readonly string Extended = T._("Erweiterte Attribute übernehmen");
        public static readonly string MoveStartStation = T._("Verschiebe nur Zeiten ab Station");
        public static readonly string Title = T._("Zug kopieren/verschieben");
    }
}