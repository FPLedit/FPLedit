using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class TrainEditForm : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private TextBox nameTextBox, commentTextBox;
        private CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
        private ComboBox locomotiveComboBox, mbrComboBox, lastComboBox;
#pragma warning restore CS0649
        private NotEmptyValidator nameValidator;

        public Station Station { get; set; }

        public Train Train { get; set; }

        private CheckBox[] daysBoxes;

        private TrainEditForm(Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            nameValidator = new NotEmptyValidator(nameTextBox);
            nameValidator.ErrorMessage = "Bitte einen Zugnamen eingeben!";

            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };

            locomotiveComboBox.Items.AddRange(GetAllTfzs(tt));
            lastComboBox.Items.AddRange(GetAllLast(tt));
            mbrComboBox.Items.AddRange(GetAllMbr(tt));

            KeyDown += (s, e) =>
            {
                if (!e.Control)
                    return;

                if (new[] { Keys.D0, Keys.Keypad0 }.Contains(e.Key))
                    zShortcutButton_Click(null, null);
                if (e.Key == Keys.A)
                    aShortcutButton_Click(null, null);
                else if (e.Key == Keys.W && e.Shift)
                    wExclSaShortcutButton_Click(null, null);
                else if (e.Key == Keys.W)
                    wShortcutButton_Click(null, null);
                else if (e.Key == Keys.S)
                    sShortcutButton_Click(null, null);

                if (new[] { Keys.D0, Keys.Keypad0, Keys.A, Keys.W, Keys.S }.Contains(e.Key))
                    e.Handled = true;
            };
        }

        public TrainEditForm(Train train) : this(train._parent)
        {
            Train = train;
            nameTextBox.Text = train.TName;
            locomotiveComboBox.Text = train.Locomotive;
            mbrComboBox.Text = train.Mbr;
            lastComboBox.Text = train.Last;
            commentTextBox.Text = train.Comment;

            for (int i = 0; i < Train.Days.Length; i++)
                daysBoxes[i].Checked = Train.Days[i];

            Title = "Zug bearbeiten";
        }

        public TrainEditForm(Timetable tt, TrainDirection direction) : this(tt)
        {
            Train = new Train(direction, tt);
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
            Train.Days = daysBoxes.Select(b => b.Checked.Value).ToArray();

            Result = DialogResult.Ok;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }

        #region Shortcut buttons

        private void wShortcutButton_Click(object sender, EventArgs e)
        {
            zShortcutButton_Click(null, null);
            daysBoxes.Take(6).All(c => { c.Checked = true; return true; });
        }

        private void wExclSaShortcutButton_Click(object sender, EventArgs e)
        {
            zShortcutButton_Click(null, null);
            daysBoxes.Take(5).All(c => { c.Checked = true; return true; });
        }

        private void sShortcutButton_Click(object sender, EventArgs e)
        {
            zShortcutButton_Click(null, null);
            daysBoxes.Last().Checked = true;
        }

        private void aShortcutButton_Click(object sender, EventArgs e)
        {
            daysBoxes.All(c => { c.Checked = true; return true; });
        }

        private void zShortcutButton_Click(object sender, EventArgs e)
        {
            daysBoxes.All(c => { c.Checked = false; return true; });
        }

        #endregion

        private IEnumerable<ListItem> GetAllTfzs(Timetable tt)
        {
            return tt.Trains
                .Select(t => t.Locomotive)
                .Distinct()
                .Where(s => s != "")
                .Select(s => new ListItem() { Text = s });
        }

        private IEnumerable<ListItem> GetAllLast(Timetable tt)
        {
            return tt.Trains
                .Select(t => t.Last)
                .Distinct()
                .Where(s => s != "")
                .Select(s => new ListItem() { Text = s });
        }

        private IEnumerable<ListItem> GetAllMbr(Timetable tt)
        {
            return tt.Trains
                .Select(t => t.Mbr)
                .Distinct()
                .Where(s => s != "")
                .Select(s => new ListItem() { Text = s });
        }
    }
}
