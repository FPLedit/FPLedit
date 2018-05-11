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
    internal class TrainCopyDialog : Dialog<DialogResult>
    {
        #pragma warning disable CS0649
        private TextBox offsetTextBox, nameTextBox, changeTextBox, countTextBox;
        private CheckBox copyAllCheckBox;
        private RadioButton copyRadioButton;
        private TableLayout extendedOptionsTable, copyOptionsTable;
        #pragma warning restore CS0649
        private NumberValidator offsetValidator, countValidator, changeValidator;

        private Train train;
        private Timetable tt;

        public TrainCopyDialog(Train t, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            offsetValidator = new NumberValidator(offsetTextBox, false, true);
            offsetValidator.ErrorMessage = "Bitte die Verscheibung als Zahl in Minuten angeben!";
            countValidator = new NumberValidator(countTextBox, false, true);
            countValidator.ErrorMessage = "Bitte eine gültige Anzahl neuer Züge eingeben!";
            changeValidator = new NumberValidator(changeTextBox, false, true);
            changeValidator.ErrorMessage = "Bitte eine gülzige Veränderung der Zugnummer eingeben!";

            train = t;
            this.tt = tt;
            nameTextBox.Text = t.TName;
            offsetTextBox.Text = "+20";
            countTextBox.Text = "1";
            changeTextBox.Text = "2";
        }

        private void SelectMode(object sender, EventArgs e)
        {
            var copy = copyRadioButton.Checked;

            extendedOptionsTable.Visible = copyOptionsTable.Visible = copy;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            var copy = copyRadioButton.Checked;

            if (!offsetValidator.Valid || (copy && (!countValidator.Valid || !changeValidator.Valid)))
            {
                MessageBox.Show("Bitte erst alle Felder korrekt ausfüllen!");
                Result = DialogResult.None;
                return;
            }

            var th = new TrainEditHelper();
            var offset = int.Parse(offsetTextBox.Text);

            if (copy)
            {
                var count = int.Parse(countTextBox.Text);
                var add = int.Parse(changeTextBox.Text);

                var trains = th.CopyTrainMultiple(train, offset, nameTextBox.Text, copyAllCheckBox.Checked.Value, count, add);

                foreach (var newTrain in trains)
                    tt.AddTrain(newTrain, true);
            }
            else
                th.MoveTrain(train, offset);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
