using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainFillDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown trainsComboBox;
        private readonly TextBox offsetTextBox;
#pragma warning restore CS0649
        private readonly NumberValidator offsetValidator;

        public Train ReferenceTrain { get; private set; }

        public int Offset { get; private set; }

        public TrainFillDialog(Train train)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            offsetValidator = new NumberValidator(offsetTextBox, false, true, errorMessage: T._("Bitte die Verschiebung als Zahl in Minuten angeben!"));

            offsetTextBox.Text = "+20";

            trainsComboBox.ItemTextBinding = Binding.Property<Train, string>(t => t.TName);
            trainsComboBox.DataStore = new TrainEditHelper().FillCandidates(train);
            trainsComboBox.SelectedIndex = 0;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!offsetValidator.Valid)
            {
                MessageBox.Show(T._("Bitte erst alle Felder korrekt ausfüllen:\n{0}", offsetValidator.ErrorMessage));
                Result = DialogResult.None;
                return;
            }

            ReferenceTrain = (Train)trainsComboBox.SelectedValue;
            Offset = int.Parse(offsetTextBox.Text);

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Zeiten übernehmen");
            public static readonly string Reference = T._("Referrenzzug:");
            public static readonly string Offset = T._("Taktverschiebung in Minuten:");
            public static readonly string Title = T._("Fahrtzeiten von anderem Zug übernehmen");
        }
    }
}
