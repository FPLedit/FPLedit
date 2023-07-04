using Eto.Forms;
using FPLedit.Editor.TimetableEditor;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using FPLedit.Shared.TrainLinks;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly TextBox nameTextBox = default!, commentTextBox = default!;
        private readonly ComboBox locomotiveComboBox = default!, mbrComboBox = default!, lastComboBox = default!;
        private readonly Button fillButton = default!, resetTransitionButton = default!;
        private readonly SingleTimetableEditControl editor = default!;
        private readonly DropDown transitionDropDown = default!;
        private readonly DaysControlNarrow daysControl = default!;
        private readonly GridView linkGridView = default!;
        private readonly GroupBox linkGroupBox = default!;
#pragma warning restore CS0649,CA2213
        private readonly NotEmptyValidator nameValidator;

        /// <summary>
        /// Output parameter for transitions. This property is populated after the form is closed.
        /// </summary>
        public Train Train { get; }
        
        /// <summary>
        /// Output parameter for transitions. This property is populated after the form is closed.
        /// </summary>
        public List<TransitionEntry> NextTrains { get; private set; }

        private readonly Timetable tt;
        private readonly TrainEditHelper th;

        private Dictionary<Station, ArrDep> arrDepBackup = null!;

        private TransitionEntry? singleTransition;

        private TrainEditForm(Timetable tt)
        {
            Train = null!; // will be initialized later.
            NextTrains = new List<TransitionEntry>();
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.tt = tt;

            th = new TrainEditHelper();

            nameValidator = new NotEmptyValidator(nameTextBox, errorMessage: T._("Bitte einen Zugnamen eingeben!"));

            locomotiveComboBox.Items.AddRange(GetAllItems(tt, t => t.Locomotive));
            lastComboBox.Items.AddRange(GetAllItems(tt, t => t.Last));
            mbrComboBox.Items.AddRange(GetAllItems(tt, t => t.Mbr));

            KeyDown += (_, e) => daysControl.HandleKeypress(e);

            resetTransitionButton.TextColor = Colors.Red;

            if (tt.Type == TimetableType.Network && tt.Version.CompareTo(TimetableVersion.Extended_FPL2) < 0)
                linkGroupBox.Visible = false;
            else
            {
                linkGridView.AddFuncColumn<TrainLink>(tl => tl.TrainCount.ToString(), T._("Anzahl"));
                linkGridView.AddFuncColumn<TrainLink>(tl => tl.TimeOffset.ToTimeString(), T._("Erster Abstand"));
                linkGridView.AddFuncColumn<TrainLink>(tl => tl.TimeDifference.ToTimeString(), T._("Zeitdifferenz"));
            }
        }

        /// <summary>
        /// Create a new instance to edit an existing train.
        /// </summary>
        /// <remarks>Don't use <see cref="NextTrains"/> in this case, transitions will be saved as well.</remarks>
        public TrainEditForm(Train train) : this(train.ParentTimetable)
        {
            Train = train;
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
        /// Use <see cref="NextTrains"/> to wire up transitions after this form has been closed.
        /// This form will NOT wire up transitions itself for new trains!
        /// </remarks>
        public TrainEditForm(Timetable tt, TrainDirection direction, List<Station>? path = null) : this(tt)
        {
            Train = new Train(direction, tt);

            if (path != null)
                Train.AddAllArrDeps(path);
            if (tt.Type == TimetableType.Linear)
                Train.AddLinearArrDeps();

            InitializeTrain();
        }

        private void InitializeTrain()
        {
            editor.Initialize(Train);

            transitionDropDown.ItemTextBinding = Binding.Delegate<Train, string>(t => t.TName);
            transitionDropDown.DataStore = tt.Trains.Where(t => t != Train).OrderBy(t => t.TName).ToArray();

            var transitions = tt.GetEditableTransitions(Train);
            // We currently only support editing complex transitions if only one transition exists (and thus not being relly "complex"...)
            if (transitions.Count == 1)
            {
                singleTransition = transitions.Single();
                transitionDropDown.SelectedValue = singleTransition.NextTrain;
            }
            else if (transitions.Count == 0)
                transitionDropDown.Enabled = true; // If no transition exists, we can certainly create one.
            else
            {
                transitionDropDown.Enabled = false;
                MessageBox.Show(T._("Dieser Zug hat komplexe Folgezüge/mehr als einen Folgezug definiert. Dies wird von FPLedit aktuell nicht zur Bearbeitung unterstützt."), "FPLedit", MessageBoxType.Warning);
            }

            fillButton.Visible = tt.Type == TimetableType.Linear && th.FillCandidates(Train).Any();

            arrDepBackup = Train.GetArrDepsUnsorted()
                .Select(kvp => new KeyValuePair<Station, ArrDep>(kvp.Key, kvp.Value.Copy()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (tt.Type != TimetableType.Network || tt.Version.CompareTo(TimetableVersion.Extended_FPL2) >= 0)
                linkGridView.DataStore = Train.TrainLinks;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!nameValidator.Valid)
            {
                MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", nameValidator.ErrorMessage ?? ""));
                return;
            }

            var nameExists = Train.ParentTimetable.Trains.Where(t => t != Train).Select(t => t.TName).Contains(nameTextBox.Text);

            if (nameExists)
            {
                if (MessageBox.Show(T._("Ein Zug mit dem Namen \"{0}\" ist bereits vorhanden. Wirklich fortfahren?", nameTextBox.Text), "FPLedit",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            Train.TName = nameTextBox.Text;
            Train.Locomotive = locomotiveComboBox.Text;
            Train.Mbr = mbrComboBox.Text;
            Train.Last = lastComboBox.Text;
            Train.Comment = commentTextBox.Text;
            Train.Days = daysControl.SelectedDays;

            if (!editor.ApplyChanges())
                return;

            NextTrains = new List<TransitionEntry>();
            // Only update NextTrains/transitions if we could actually select a transition.
            // Otherwise we keep the existing complex structure.
            if (transitionDropDown.Enabled)
            {
                if (transitionDropDown.SelectedValue != null)
                {
                    var next = (ITrain) transitionDropDown.SelectedValue;
                    if (singleTransition != null)
                    {
                        // keep the old complex structure and only replace the next train.
                        singleTransition.NextTrain = next;
                        NextTrains.Add(singleTransition);
                    }
                    else
                        NextTrains.Add(new TransitionEntry(next, Days.All, null));
                }

                if (Train.Id > 0)
                    tt.SetTransitions(Train, NextTrains);
            }

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            foreach (var (sta, ardpBak) in arrDepBackup)
                Train.GetArrDep(sta).ApplyCopy(ardpBak);
            Close(DialogResult.Cancel);
        }

        private void FillButton_Click(object sender, EventArgs e)
        {
            using var tfd = new TrainFillDialog(Train);
            if (tfd.ShowModal() == DialogResult.Ok)
            {
                th.FillTrain(tfd.ReferenceTrain!, Train, tfd.Offset);

                editor.Initialize(Train);
            }
        }

        private void ResetTransitionButton_Click(object sender, EventArgs e)
        {
            transitionDropDown.SelectedIndex = -1;
        }

        private void DeleteLinkButton_Click(object sender, EventArgs e)
        {
            if (tt.Type == TimetableType.Network && tt.Version.CompareTo(TimetableVersion.Extended_FPL2) < 0)
                throw new TimetableTypeNotSupportedException("train links");
            
            if (linkGridView.SelectedItem != null)
            {
                tt.RemoveLink((TrainLink) linkGridView.SelectedItem);
                linkGridView.DataStore = Train.TrainLinks; // Reload data rows.
            }
            else
                MessageBox.Show(T._("Erst muss eine Verknüpfung zum Löschen ausgewählt werden!"));
        }
        
        private void EditLinkButton_Click(object sender, EventArgs e)
        {
            if (tt.Type == TimetableType.Network && tt.Version.CompareTo(TimetableVersion.Extended_FPL2) < 0)
                throw new TimetableTypeNotSupportedException("train links");
            
            if (linkGridView.SelectedItem != null)
            {
                var link = (TrainLink) linkGridView.SelectedItem;
                if (link.TrainNamingScheme is AutoTrainNameGen || link.TrainNamingScheme is SpecialTrainNameGen)
                {
                    using (var tled = new TrainLinkEditDialog(link, tt))
                        if (tled.ShowModal() == DialogResult.Ok)
                            linkGridView.DataStore = Train.TrainLinks; // Reload data rows.
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
            public static readonly string Timetable = T._("Fahrplan");
            public static readonly string Fill = T._("Von anderem Zug...");
            public static readonly string Title = T._("Neuen Zug erstellen");
        }
    }
}
