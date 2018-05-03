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
        private TextBox offsetTextBox, nameTextBox;
        private CheckBox copyAllCheckBox;
        #pragma warning restore CS0649
        private NumberValidator numberValidator;

        private Train train;
        private Timetable tt;

        public TrainCopyDialog(Train t, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            numberValidator = new NumberValidator(offsetTextBox, false, true);
            numberValidator.ErrorMessage = "Bitte die Verscheibung als Zahl in Minuten angeben!";

            train = t;
            this.tt = tt;
            nameTextBox.Text = t.TName;
            offsetTextBox.Text = "+20";
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!numberValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Felder korrekt ausfüllen!");
                Result = DialogResult.None;
                return;
            }

            var offset = int.Parse(offsetTextBox.Text);

            var th = new TrainCopyHelper();
            var newTrain = th.CopyTrain(train, offset, nameTextBox.Text, copyAllCheckBox.Checked.Value);
            tt.AddTrain(newTrain, true);

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
