using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

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

            offsetValidator = new NumberValidator(offsetTextBox, false, true, errorMessage: T._("Bitte die Verschiebung als Zahl in Minuten angeben!"));
            countValidator = new NumberValidator(countTextBox, false, true, allowNegative: false, errorMessage: T._("Bitte eine gültige Anzahl >0 neuer Züge eingeben!"));
            changeValidator = new NumberValidator(changeTextBox, false, true, errorMessage: T._("Bitte eine gültige Veränderung der Zugnummer eingeben!"));

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
            extendedOptionsTable.Visible = copyOptionsTable.Visible = (mode == CopySelectionMode.Copy || modeSelect.SelectedState == CopySelectionMode.Link);
            copyAllCheckBox.Visible = mode == CopySelectionMode.Copy;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            var copy = modeSelect.SelectedState == CopySelectionMode.Copy || modeSelect.SelectedState == CopySelectionMode.Link;

            if (!offsetValidator.Valid || (copy && (!countValidator.Valid || !changeValidator.Valid)))
            {
                var msg = offsetValidator.Valid ? "" : offsetValidator.ErrorMessage + Environment.NewLine;
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
            var offset = int.Parse(offsetTextBox.Text);

            if (modeSelect.SelectedState == CopySelectionMode.Copy)
            {
                var count = int.Parse(countTextBox.Text);
                var add = int.Parse(changeTextBox.Text);

                var trains = th.CopyTrainMultiple(train, offset, nameTextBox.Text, copyAllCheckBox.Checked.Value, count, add);

                foreach (var newTrain in trains)
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
                var add = int.Parse(changeTextBox.Text);

                th.LinkTrainMultiple(train, offset, nameTextBox.Text, count, add);
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
            Move,
            [SelectionName("Zug verlinken")]
            Link
        }
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Kopieren");
            public static readonly string Offset = T._("Taktverschiebung in Minuten");
            public static readonly string Count = T._("Anzahl der neuen Züge");
            public static readonly string BaseName = T._("Basiszugnummer");
            public static readonly string NameChange = T._("Änderung der Zugnummer");
            public static readonly string Extended = T._("Erweiterte Attribute übernehmen");
            public static readonly string Title = T._("Zug kopieren/verschieben");
        }
    }
}
