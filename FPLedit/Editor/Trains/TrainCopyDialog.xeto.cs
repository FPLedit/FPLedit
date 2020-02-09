using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainCopyDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly TextBox offsetTextBox, nameTextBox, changeTextBox, countTextBox;
        private readonly CheckBox copyAllCheckBox;
        private readonly StackLayout selectStack;
        private readonly TableLayout extendedOptionsTable, copyOptionsTable;
#pragma warning restore CS0649
        private readonly NumberValidator offsetValidator, countValidator, changeValidator;
        private readonly SelectionUI<CopySelectionMode> modeSelect;

        private readonly Train train;
        private readonly Timetable tt;

        public TrainCopyDialog(Train t, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            offsetValidator = new NumberValidator(offsetTextBox, false, true, errorMessage: "Bitte die Verschiebung als Zahl in Minuten angeben!");
            countValidator = new NumberValidator(countTextBox, false, true, errorMessage: "Bitte eine gültige Anzahl neuer Züge eingeben!");
            changeValidator = new NumberValidator(changeTextBox, false, true, errorMessage: "Bitte eine gültige Veränderung der Zugnummer eingeben!");

            train = t;
            this.tt = tt;
            nameTextBox.Text = t.TName;
            offsetTextBox.Text = "+20";
            countTextBox.Text = "1";
            changeTextBox.Text = "2";

            modeSelect = new SelectionUI<CopySelectionMode>(SelectMode, selectStack);
        }

        private void SelectMode(CopySelectionMode mode)
        {
            var copy = mode == CopySelectionMode.Copy;

            extendedOptionsTable.Visible = copyOptionsTable.Visible = copy;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            var copy = modeSelect.SelectedState == 0;

            if (!offsetValidator.Valid || (copy && (!countValidator.Valid || !changeValidator.Valid)))
            {
                var msg = offsetValidator.Valid ? "" : offsetValidator.ErrorMessage + Environment.NewLine;
                if (copy)
                {
                    msg += countValidator.Valid ? "" : countValidator.ErrorMessage + Environment.NewLine;
                    msg += changeValidator.Valid ? "" : changeValidator.ErrorMessage + Environment.NewLine;
                }
                MessageBox.Show("Bitte erst alle Felder korrekt ausfüllen:" + Environment.NewLine + msg);
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
                {
                    if (tt.Trains.Any(t => t.TName == newTrain.TName))
                    {
                        if (MessageBox.Show($"Es ist bereits ein Zug mit der Zugnummer {newTrain.TName} in diesem Fahrplan vorhanden, soll diese Kopie trotzdem angelegt werden?",
                            "Züge kopieren", MessageBoxButtons.YesNo) == DialogResult.No)
                            continue;
                    }

                    tt.AddTrain(newTrain, true);
                }
            }
            else
                th.MoveTrain(train, offset);

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        protected override void Dispose(bool disposing)
        {
            modeSelect?.Dispose();
            base.Dispose(disposing);
        }

        private enum CopySelectionMode
        {
            [SelectionName("Zug kopieren")]
            Copy,
            [SelectionName("Zug verschieben")]
            Move
        }
    }
}
