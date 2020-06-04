using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;

namespace FPLedit.Shared.UI
{
    public sealed class FontComboBox
    {
        private readonly ComboBox box;
        private readonly Label label;

        public FontComboBox(ComboBox box, Label label)
        {
            this.box = box;
            this.label = label;

            box.DataStore = new [] { "<Lade>" };
            box.SelectedIndex = 0;

            // Asynchrones Laden der Font-Liste, um Performance-Problemen vorzubeugen
            Application.Instance.AsyncInvoke(() =>
            {
                box.ItemTextBinding = Binding.Property<string, string>(s => s);
                box.DataStore = FontCollection.Families;
            });

            box.TextChanged += TextChanged;
        }

        private void TextChanged(object? sender, EventArgs e)
        {
            if (box.Text == "" || label == null)
                return;

            try
            {
                label.Font = new Font(box.Text, 10);
            }
            catch
            {
                label.Visible = false;
            }
        }
    }
}
