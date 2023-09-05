using Eto.Forms;
using FPLedit.Shared.UI;
using System;
using FPLedit.Shared;

namespace FPLedit.Editor.TimetableEditor;

internal sealed class ZlmEditForm : FDialog<string?>
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox zlmTextBox = default!;
#pragma warning restore CS0649,CA2213

    public ZlmEditForm(string zlm)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);
        zlmTextBox.Text = zlm;
    }

    private void CloseButton_Click(object sender, EventArgs e)
        => Close(zlmTextBox.Text);

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(null);

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Position = T._("Position (km)");
        public static readonly string Title = T._("Zuglaufmeldung durch");
    }
}