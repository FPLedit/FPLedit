using Eto.Forms;
using FPLedit.Editor.TimetableEditor;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FPLedit.Shared.TrainLinks;

namespace FPLedit.Editor.Trains;

internal sealed class TrainEditForm : FDialog<TrainEditForm.EditResult?>
{
    internal sealed record EditResult(Train Train, IEnumerable<TransitionEntry> NextTrains);

#pragma warning disable CS0649,CA2213
    private readonly TextBox nameTextBox = default!, commentTextBox = default!;
    private readonly ComboBox locomotiveComboBox = default!, mbrComboBox = default!, lastComboBox = default!;
    private readonly Button fillButton = default!;
    private readonly SingleTimetableEditControl editor = default!;
    private readonly DaysControlNarrow daysControl = default!;
    private readonly GridView linkGridView = default!, transitionsGridView = default!;
#pragma warning restore CS0649,CA2213
    private readonly NotEmptyValidator nameValidator;

    private readonly Train train;
    private readonly ObservableCollection<TransitionEntry> transitions = new ();

    private readonly Timetable tt;
    private readonly TrainEditHelper th;

    private Dictionary<Station, ArrDep> arrDepBackup = null!;

    private TrainEditForm(Timetable tt)
    {
        train = null!; // will be initialized later.
        Eto.Serialization.Xaml.XamlReader.Load(this);

        this.tt = tt;

        th = new TrainEditHelper();

        nameValidator = new NotEmptyValidator(nameTextBox, errorMessage: T._("Bitte einen Zugnamen eingeben!"));

        locomotiveComboBox.Items.AddRange(GetAllItems(tt, t => t.Locomotive));
        lastComboBox.Items.AddRange(GetAllItems(tt, t => t.Last));
        mbrComboBox.Items.AddRange(GetAllItems(tt, t => t.Mbr));

        KeyDown += (_, e) => daysControl.HandleKeypress(e);

        linkGridView.AddFuncColumn<TrainLink>(tl => tl.TrainCount.ToString(), T._("Anzahl"));
        linkGridView.AddFuncColumn<TrainLink>(tl => tl.TimeOffset.ToTimeString(), T._("Erster Abstand"));
        linkGridView.AddFuncColumn<TrainLink>(tl => tl.TimeDifference.ToTimeString(), T._("Zeitdifferenz"));

        transitionsGridView.AddFuncColumn<TransitionEntry>(tl => tl.NextTrain.TName, T._("Folgezug"));
        transitionsGridView.AddFuncColumn<TransitionEntry>(tl => tl.Days.ToBinString(), T._("Tage"));
        transitionsGridView.AddFuncColumn<TransitionEntry>(tl => tl.Station?.SName ?? "-", T._("Station"));
    }

    /// <summary>
    /// Create a new instance to edit an existing train.
    /// </summary>
    /// <remarks>Don't use <see cref="EditResult.NextTrains"/> in this case, transitions will be saved as well.</remarks>
    public TrainEditForm(Train train) : this(train.ParentTimetable)
    {
        this.train = train;
        nameTextBox.Text = train.TName;
        locomotiveComboBox.Text = train.Locomotive;
        mbrComboBox.Text = train.Mbr;
        lastComboBox.Text = train.Last;
        commentTextBox.Text = train.Comment;
        daysControl.SelectedDays = train.Days;

        Title = T._("Zug bearbeiten");

        InitializeTrain();
    }

    /// <summary>
    /// Create a new instance to create a new train.
    /// </summary>
    /// <remarks>
    /// Use <see cref="EditResult.NextTrains"/> to wire up transitions after this form has been closed.
    /// This form will NOT wire up transitions itself for new trains!
    /// </remarks>
    public TrainEditForm(Timetable tt, TrainDirection direction, IEnumerable<Station>? path = null) : this(tt)
    {
        train = new Train(direction, tt);

        if (path != null)
            train.AddAllArrDeps(path);
        if (tt.Type == TimetableType.Linear)
            train.AddLinearArrDeps();

        InitializeTrain();
    }

    private void InitializeTrain()
    {
        editor.Initialize(train);

        transitions.Clear();
        foreach (var tr in tt.GetEditableTransitions(train))
            transitions.Add(tr);
        transitionsGridView.DataStore = transitions;

        fillButton.Visible = tt.Type == TimetableType.Linear && th.FillCandidates(train).Any();

        arrDepBackup = train.GetArrDepsUnsorted()
            .Select(kvp => new KeyValuePair<Station, ArrDep>(kvp.Key, kvp.Value.Copy()))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        linkGridView.DataStore = train.TrainLinks;
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (!nameValidator.Valid)
        {
            MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", nameValidator.ErrorMessage ?? ""));
            return;
        }

        var nameExists = train.ParentTimetable.Trains.Where(t => t != train).Select(t => t.TName).Contains(nameTextBox.Text);

        if (nameExists)
        {
            if (MessageBox.Show(T._("Ein Zug mit dem Namen \"{0}\" ist bereits vorhanden. Wirklich fortfahren?", nameTextBox.Text), "FPLedit",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                return;
        }

        train.TName = nameTextBox.Text;
        train.Locomotive = locomotiveComboBox.Text;
        train.Mbr = mbrComboBox.Text;
        train.Last = lastComboBox.Text;
        train.Comment = commentTextBox.Text;
        train.Days = daysControl.SelectedDays;

        if (!editor.ApplyChanges())
            return;

        if (train.Id > 0)
            tt.SetTransitions(train, transitions);

        Close(new EditResult(train, transitions));
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        foreach (var (sta, ardpBak) in arrDepBackup)
            train.GetArrDep(sta).ApplyCopy(ardpBak);
        Close(null);
    }

    private void FillButton_Click(object sender, EventArgs e)
    {
        using var tfd = new TrainFillDialog(train);
        var fillOperation = tfd.ShowModal();
        if (fillOperation != null)
        {
            th.FillTrain(fillOperation.ReferenceTrain, train, fillOperation.Offset);
            editor.Initialize(train);
        }
    }

    private void DeleteTransitionButton_Click(object sender, EventArgs e)
    {
        if (transitionsGridView.SelectedItem != null)
            transitions.Remove((TransitionEntry) transitionsGridView.SelectedItem);
        else
            MessageBox.Show(T._("Erst muss ein Folgezug zum Löschen ausgewählt werden!"));
    }

    private void EditTransitionButton_Click(object sender, EventArgs e)
    {
        if (transitionsGridView.SelectedItem != null)
        {
            var transition = (TransitionEntry) transitionsGridView.SelectedItem;
            var tef = new TrainTransitionEditDialog(transition, train, tt);
            var newTransition = tef.ShowModal();
            if (newTransition != null)
            {
                // TransitionEntry is a record, so == does check data equality. Do not match the original transition entry.
                var duplicateTransition = transitions.Any(t => t == newTransition && !ReferenceEquals(t, transition));
                if (!duplicateTransition)
                {
                    transitions.Insert(transitions.IndexOf(transition), newTransition);
                    transitions.Remove(transition);
                }
                else
                    MessageBox.Show(T._("Ein Folgezug mit den gleichen Angaben existiert bereits!"));
            }
        }
        else
            MessageBox.Show(T._("Erst muss ein Folgezug zum Bearbeiten ausgewählt werden!"));
    }

    private void AddTransitionButton_Click(object sender, EventArgs e)
    {
        var tef = new TrainTransitionEditDialog(null, train, tt);
        var newTransition = tef.ShowModal();
        if (newTransition != null)
        {
            // TransitionEntry is a record, so == does check data equality.
            var duplicateTransition = transitions.Any(t => t == newTransition);
            if (!duplicateTransition)
                transitions.Add(newTransition);
            else
                MessageBox.Show(T._("Ein Folgezug mit den gleichen Angaben existiert bereits!"));
        }
    }

    private void DeleteLinkButton_Click(object sender, EventArgs e)
    {
        if (linkGridView.SelectedItem != null)
        {
            tt.RemoveLink((TrainLink) linkGridView.SelectedItem);
            linkGridView.DataStore = train.TrainLinks; // Reload data rows.
        }
        else
            MessageBox.Show(T._("Erst muss eine Verknüpfung zum Löschen ausgewählt werden!"));
    }

    private void EditLinkButton_Click(object sender, EventArgs e)
    {
        if (linkGridView.SelectedItem != null)
        {
            var link = (TrainLink) linkGridView.SelectedItem;
            if (link.TrainNamingScheme is AutoTrainNameGen or SpecialTrainNameGen)
            {
                using var tled = new TrainLinkEditDialog(link, tt);
                if (tled.ShowModal() == DialogResult.Ok)
                    linkGridView.DataStore = train.TrainLinks; // Reload data rows.
            }
            else
                MessageBox.Show("Not Implemented: Name calculator not supported!", type: MessageBoxType.Error);
        }
        else
            MessageBox.Show(T._("Erst muss eine Verknüpfung zum Bearbeiten ausgewählt werden!"));
    }

    private IEnumerable<ListItem> GetAllItems(ITimetable timetable, Func<ITrain, string> func)
        => timetable.Trains.Select(func).Distinct().Where(s => s != "").OrderBy(s => s).Select(s => new ListItem { Text = s }).ToArray();

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Name = T._("Name");
        public static readonly string Comment = T._("Kommentar");
        public static readonly string Extended = T._("Erweiterte Eigenschaften (für Bfpl)");
        public static readonly string Tfz = T._("Tfz");
        public static readonly string TfzTooltip = T._("Die Zuglok oder der verwendete Triebwagen.");
        public static readonly string Mbr = T._("Mbr");
        public static readonly string MbrTooltip = T._("Mindesbremshundertstel");
        public static readonly string Last = T._("Last");
        public static readonly string Days = T._("Verkehrstage");
        public static readonly string NextTrain = T._("Folgezug");
        public static readonly string Links = T._("Verknüpfungen");
        public static readonly string DeleteLink = T._("Verknüpfung löschen");
        public static readonly string EditLink = T._("Verknüpfung bearbeiten");
        public static readonly string NewTransition = T._("Neuer Folgezug");
        public static readonly string EditTransition = T._("Folgezug bearbeiten");
        public static readonly string DeleteTransition = T._("Folgezug löschen");
        public static readonly string Timetable = T._("Fahrplan");
        public static readonly string Fill = T._("Von anderem Zug...");
        public static readonly string Title = T._("Neuen Zug erstellen");
    }
}