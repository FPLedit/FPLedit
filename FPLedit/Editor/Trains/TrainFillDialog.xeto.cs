using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Linq;

namespace FPLedit.Editor.Trains;

internal sealed class TrainFillDialog : FDialog<TrainFillDialog.FillOperation?>
{
    internal sealed record FillOperation(Train ReferenceTrain, int Offset);

#pragma warning disable CS0649,CA2213
    private readonly DropDown trainsComboBox = default!;
    private readonly TextBox offsetTextBox = default!;
#pragma warning restore CS0649,CA2213
    private readonly NumberValidator offsetValidator;

    public TrainFillDialog(Train train)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        offsetValidator = new NumberValidator(offsetTextBox, false, true, errorMessage: T._("Bitte die Verschiebung als Zahl in Minuten angeben!"));

        offsetTextBox.Text = "+20";

        trainsComboBox.ItemTextBinding = Binding.Delegate<Train, string>(t => t.TName);
        trainsComboBox.DataStore = new TrainEditHelper().FillCandidates(train).ToArray();
        trainsComboBox.SelectedIndex = 0;
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (!offsetValidator.Valid)
        {
            MessageBox.Show(T._("Bitte erst alle Felder korrekt ausfüllen:\n{0}", offsetValidator.ErrorMessage!));
            Result = null;
            return;
        }

        Close(new FillOperation((Train)trainsComboBox.SelectedValue, int.Parse(offsetTextBox.Text)));
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(null);
        
    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Zeiten übernehmen");
        public static readonly string Reference = T._("Referrenzzug:");
        public static readonly string Offset = T._("Taktverschiebung in Minuten:");
        public static readonly string Title = T._("Fahrtzeiten von anderem Zug übernehmen");
    }
}