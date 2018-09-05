using Eto.Drawing;
using Eto.Forms;
using System;
using System.Drawing.Text;
using System.Linq;

namespace FPLedit.Shared.UI
{
    public class FontComboBox
    {
        private ComboBox box;
        private Label label;

        public FontComboBox(ComboBox box, Label label)
        {
            this.box = box;
            this.label = label;

            box.DataStore = new InstalledFontCollection().Families.Select(f => f.Name).OrderBy(f => f).ToArray();
            box.ItemTextBinding = Binding.Property<string, string>(s => s);
            box.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            if (box.Text == "" || label == null)
                return;

            try
            {
                label.Font = new Font(box.Text, 10);
            }
            catch { }
        }
    }
}
