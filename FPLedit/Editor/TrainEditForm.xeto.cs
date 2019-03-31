using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor
{
    internal class TrainEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private TextBox nameTextBox, commentTextBox;
        private CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
        private ComboBox locomotiveComboBox, mbrComboBox, lastComboBox;
        private Button wShort, wSaShort, sShort, aShort, zShort, fillButton;
        private Network.SingleTimetableEditControl editor;
        private DropDown transitionDropDown;
        private GroupBox transitionsGroupBox;
#pragma warning restore CS0649
        private NotEmptyValidator nameValidator;

        public Station Station { get; set; }

        public Train Train { get; set; }

        private CheckBox[] daysBoxes;
        private ToggleButton[] shortcutsToggle;
        private Timetable tt;
        private TrainEditHelper th;

        private Dictionary<Station, ArrDep> arrDepBackup;

        private Days wShortcut = Days.Parse("1111110");
        private Days wExclSaShortcut = Days.Parse("1111100");
        private Days sShortcut = Days.Parse("0000001");
        private Days aShortcut = Days.Parse("1111111");
        private Days zShortcut = Days.Parse("0000000");

        private TrainEditForm(Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.tt = tt;

            th = new TrainEditHelper();

            nameValidator = new NotEmptyValidator(nameTextBox);
            nameValidator.ErrorMessage = "Bitte einen Zugnamen eingeben!";

            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };
            foreach (var dayBox in daysBoxes)
                dayBox.CheckedChanged += CheckBoxStateChanged;

            locomotiveComboBox.Items.AddRange(GetAllItems(tt, t => t.Locomotive));
            lastComboBox.Items.AddRange(GetAllItems(tt, t => t.Last));
            mbrComboBox.Items.AddRange(GetAllItems(tt, t => t.Mbr));

            KeyDown += (s, e) =>
            {
                if (!e.Control)
                    return;

                var handled = true;
                if (new[] { Keys.D0, Keys.Keypad0 }.Contains(e.Key))
                    ApplyShortcut(zShortcut);
                else if (e.Key == Keys.A)
                    ApplyShortcut(aShortcut);
                else if (e.Key == Keys.W && e.Shift)
                    ApplyShortcut(wExclSaShortcut);
                else if (e.Key == Keys.W)
                    ApplyShortcut(wShortcut);
                else if (e.Key == Keys.S)
                    ApplyShortcut(sShortcut);
                else
                    handled = false;

                e.Handled = handled;
            };

            var shortcutsButtons = new[] { wShort, wSaShort, sShort, aShort, zShort };
            var shortcuts = new[] { wShortcut, wExclSaShortcut, sShortcut, aShortcut, zShortcut };
            shortcutsToggle = new ToggleButton[shortcuts.Length];
            for (int i = 0; i < shortcutsButtons.Length; i++)
            {
                var toggle = new ToggleButton(shortcutsButtons[i])
                {
                    Tag = shortcuts[i],
                    AllowDisable = false
                };
                toggle.ToggleClick += ApplyShortcutBtn;
                shortcutsToggle[i] = toggle;
            }
        }

        public TrainEditForm(Train train) : this(train._parent)
        {
            Train = train;
            nameTextBox.Text = train.TName;
            locomotiveComboBox.Text = train.Locomotive;
            mbrComboBox.Text = train.Mbr;
            lastComboBox.Text = train.Last;
            commentTextBox.Text = train.Comment;
            ApplyShortcut(Train.Days);
            CheckBoxStateChanged(null, null);

            Title = "Zug bearbeiten";

            InitializeTrain();
        }

        public TrainEditForm(Timetable tt, TrainDirection direction, List<Station> path = null) : this(tt)
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
            editor.Initialize(Train._parent, Train);

            if (tt.Version != TimetableVersion.JTG2_x)
            {
                transitionDropDown.DataStore = tt.Trains.Where(t => t != Train);
                transitionDropDown.ItemTextBinding = Binding.Property<Train, string>(t => t.TName);
                transitionDropDown.SelectedValue = tt.GetTransition(Train);
            }
            else
                transitionsGroupBox.Visible = false;

            fillButton.Visible = tt.Type == TimetableType.Linear && th.FillCandidates(Train).Any();

            arrDepBackup = Train.GetArrDeps()
                .Select(kvp => new KeyValuePair<Station, ArrDep>(kvp.Key, kvp.Value.Clone<ArrDep>()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben:" + Environment.NewLine + nameValidator.ErrorMessage);
                return;
            }

            var name_exists = Train._parent.Trains.Select(t => t.TName).Contains(nameTextBox.Text);

            if (name_exists && Train.TName == null)
            {
                if (MessageBox.Show("Ein Zug mit dem Namen \"" + nameTextBox.Text + "\" ist bereits vorhanden. Wirklich fortfahren?", "FPLedit",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            Train.TName = nameTextBox.Text;
            Train.Locomotive = locomotiveComboBox.Text;
            Train.Mbr = mbrComboBox.Text;
            Train.Last = lastComboBox.Text;
            Train.Comment = commentTextBox.Text;
            Train.Days = new Days(daysBoxes.Select(b => b.Checked.Value).ToArray());

            editor.ApplyChanges();

            if (tt.Version != TimetableVersion.JTG2_x)
                tt.SetTransition(Train, (Train)transitionDropDown.SelectedValue);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            foreach (var kvp in arrDepBackup)
                Train.GetArrDep(kvp.Key).ApplyCopy(kvp.Value);
            Close(DialogResult.Cancel);
        }

        private void fillButton_Click(object sender, EventArgs e)
        {
            var tfd = new TrainFillDialog(tt, Train);
            if (tfd.ShowModal() == DialogResult.Ok)
            {
                var th = new TrainEditHelper();
                th.FillTrain(tfd.ReferenceTrain, Train, tfd.Offset);

                editor.Initialize(Train._parent, Train);
            }
        }

        #region Shortcut buttons
        private void ApplyShortcutBtn(object sender, EventArgs e)
        {
            var btn = (ToggleButton)sender;
            if (btn.Tag is Days days)
                ApplyShortcut(days);
        }

        private void ApplyShortcut(Days days)
        {
            for (int i = 0; i < days.Length; i++)
                daysBoxes[i].Checked = days[i];
        }

        private void CheckBoxStateChanged(object sender, EventArgs e)
        {
            var cur = new Days(daysBoxes.Select(b => b.Checked.Value).ToArray());
            foreach (var item in shortcutsToggle)
                item.Checked = cur.Equals((Days)item.Tag);
        }
        #endregion

        private IEnumerable<ListItem> GetAllItems(Timetable tt, Func<Train, string> func)
            => tt.Trains.Select(func).Distinct().Where(s => s != "").Select(s => new ListItem() { Text = s });
    }
}
