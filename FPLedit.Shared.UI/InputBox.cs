using System;
using Eto.Drawing;
using Eto.Forms;

namespace FPLedit.Shared.UI
{
    public sealed class InputBox : FDialog<DialogResult>
    {
        private readonly TableLayout table;
        private readonly StackLayout stack;
        private readonly TextBox tb;
        private string? ResultText { get; set; }
        
        private InputBox(string title, string defaultValue)
        {
            var closeButton = DefaultButton = new Button(CloseButtonClick)
            {
                Text = T._("Schließen"),
            };
            var abortButton = AbortButton = new Button(AbortButtonClick)
            {
                Text = T._("Abbrechen"),
            };
            tb = new TextBox() { Text = defaultValue, };
            table = new TableLayout(new TableRow(new TableCell() { ScaleWidth = true }, abortButton, closeButton)) { Spacing = new Size(5, 5) };
            stack = new StackLayout(tb, table)
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 10,
                Spacing = 5,
            };
            Content = stack;
            Title = title;
            Resizable = false;
        }

        private void AbortButtonClick(object? s, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }

        private void CloseButtonClick(object? s, EventArgs e)
        {
            Result = DialogResult.Ok;
            ResultText = tb.Text;
            Close();
        }

        public static string? Query(Window parent, string title, string defaultValue)
        {
            using (var f = new InputBox(title, defaultValue))
            {
                if (f.ShowModal(parent) == DialogResult.Ok)
                    return f.ResultText;
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (tb != null && !tb.IsDisposed)
                tb.Dispose();
            if (table != null && !table.IsDisposed)
                table.Dispose();
            if (stack != null && !stack.IsDisposed)
                stack.Dispose();
            
            base.Dispose(disposing);
        }
    }
}