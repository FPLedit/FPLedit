using Eto.Forms;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using FPLedit.Shared;

namespace FPLedit.Editor
{
    internal sealed class EditPositionForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly TextBox positionTextBox = default!;
#pragma warning restore CS0649,CA2213
        private readonly NumberValidator positionValidator;

        public float Position { get; private set; }

        public EditPositionForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: T._("Bitte eine Zahl als Position eingeben!"));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!positionValidator.Valid)
            {
                MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", positionValidator.ErrorMessage!));
                return;
            }

            Position = float.Parse(positionTextBox.Text);

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string Position = T._("Position (km)");
            public static readonly string Title = T._("Neuen Positionseintrag bearbeiten");
        }
    }
}
