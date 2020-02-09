using Eto.Drawing;
using Eto.Forms;

namespace FPLedit.Editor.Rendering
{
    internal sealed class Divider : Panel
    {
        public Divider()
        {
            BackgroundColor = SystemColors.ControlText;
            Size = new Size(2, 23);
        }
    }
}
