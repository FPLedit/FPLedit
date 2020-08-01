using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using FPLedit.Shared;

namespace FPLedit.Editor.TimetableEditor
{
    internal sealed class ZlmEditForm : FDialog<DialogResult>
    {
        #pragma warning disable CS0649
        private readonly TextBox zlmTextBox;
        #pragma warning restore CS0649

        public string Zlm { get; set; }

        public ZlmEditForm(string zlm)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            Zlm = zlm;
            zlmTextBox.Text = zlm;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Zlm = zlmTextBox.Text;
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string Position = T._("Position (km)");
            public static readonly string Title = T._("Zuglaufmeldung durch");
        }
    }
}
