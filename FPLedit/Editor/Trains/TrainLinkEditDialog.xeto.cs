using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainLinkEditDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly TextBox startOffsetTextBox, differenceTextBox, nameTextBox, changeTextBox, countTextBox;
#pragma warning restore CS0649
        private readonly NumberValidator differenceValidator, countValidator, changeValidator;

        private readonly Train train;
        private readonly Timetable tt;
        private readonly TrainLink origLink;

        /// <summary>
        /// Create a new dialog to edit (= discard + create new) the given train link.
        /// </summary>
        /// <param name="link">Link object that is used as blueprint. Note: The referenced object will not be mutated!</param>
        /// <param name="tt">Current timetable instance.</param>
        public TrainLinkEditDialog(TrainLink link, Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            differenceValidator = new NumberValidator(differenceTextBox, false, true, errorMessage: T._("Bitte die Verschiebung als Zahl in Minuten angeben!"));
            countValidator = new NumberValidator(countTextBox, false, true, allowNegative: false, errorMessage: T._("Bitte eine gültige Anzahl >0 neuer Züge eingeben!"));
            changeValidator = new NumberValidator(changeTextBox, false, true, errorMessage: T._("Bitte eine gültige Veränderung der Zugnummer eingeben!"));

            origLink = link;
            train = link.ParentTrain;
            this.tt = tt;
            //TODO: Support more generic name calculators.
            nameTextBox.Text = (link.TrainNamingScheme as AutoTrainNameCalculator).BaseTrainName.FullName;
            startOffsetTextBox.Text = link.TimeOffset.ToString("+#;-#;0");
            differenceTextBox.Text = link.TimeDifference.ToString("+#;-#;0");
            countTextBox.Text = link.TrainCount.ToString();
            changeTextBox.Text = (link.TrainNamingScheme as AutoTrainNameCalculator).Increment.ToString();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!differenceValidator.Valid || !countValidator.Valid || !changeValidator.Valid)
            {
                var msg = differenceValidator.Valid ? "" : differenceValidator.ErrorMessage + Environment.NewLine;
                msg += countValidator.Valid ? "" : countValidator.ErrorMessage + Environment.NewLine;
                msg += changeValidator.Valid ? "" : changeValidator.ErrorMessage + Environment.NewLine;
                MessageBox.Show(T._("Bitte erst alle Felder korrekt ausfüllen:\n{0}", msg));
                Result = DialogResult.None;
                return;
            }

            var th = new TrainEditHelper();
            var offset = int.Parse(startOffsetTextBox.Text);
            var diff = int.Parse(differenceTextBox.Text);

            var count = int.Parse(countTextBox.Text);
            var add = int.Parse(changeTextBox.Text);

            tt.RemoveLink(origLink); // Remove old link.
            th.LinkTrainMultiple(train, offset, diff, nameTextBox.Text, count, add); // Create new link.

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Übernehmen");
            public static readonly string StartOffset = T._("Verschiebung des ersten Zugs");
            public static readonly string Difference = T._("Taktverschiebung in Minuten");
            public static readonly string Count = T._("Anzahl der neuen Züge");
            public static readonly string BaseName = T._("Basiszugnummer");
            public static readonly string NumberChange = T._("Änderung der Zugnummer");
            public static readonly string Title = T._("Verlinkung bearbeiten");
        }
    }
}
