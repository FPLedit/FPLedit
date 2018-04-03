using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Validators;
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
        private TextBox nameTextBox, commentTextBox, mbrTextBox, lastTextBox;
        private CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
        private ComboBox locomotiveComboBox;
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

            locomotiveComboBox.Items.AddRange(tt.GetAllTfzs().Select(s => new ListItem() { Text = s }));

            KeyDown += (s, e) =>
            {
                if (!e.Control)
                    return;

                if (new[] { Keys.A, Keys.W, Keys.S, Keys.D0, Keys.Keypad0 }.Contains(e.Key))
                {
                    daysBoxes.All(c => { c.Checked = false; return true; });
                    e.Handled = true;
                }
                if (e.Key == Keys.A)
                    daysBoxes.All(c => { c.Checked = true; return true; });
                else if (e.Key == Keys.W && e.Shift)
                    daysBoxes.Take(5).All(c => { c.Checked = true; return true; });
                else if (e.Key == Keys.W)
                    daysBoxes.Take(6).All(c => { c.Checked = true; return true; });
                else if (e.Key == Keys.S)
                    daysBoxes.Last().Checked = true;
            };
        }

        public TrainEditForm(Train train) : this(train._parent)
        {
            Train = train;
            nameTextBox.Text = train.TName;
            locomotiveComboBox.Text = train.Locomotive;
            mbrTextBox.Text = train.Mbr;
            lastTextBox.Text = train.Last;
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
                MessageBox.Show("Bitte erst alle Fehler beheben!");
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
            Train.Mbr = mbrTextBox.Text;
            Train.Last = lastTextBox.Text;
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
    }
}
