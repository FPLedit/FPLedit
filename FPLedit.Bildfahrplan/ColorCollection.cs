using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport
{
    public class ColorCollection
    {
        private Dictionary<string, Color> colors = new Dictionary<string, Color>()
        {
            ["Schwarz"] = Color.Black,
            ["Grau"] = Color.Gray,
            ["Weiß"] = Color.White,
            ["Rot"] = Color.Red,
            ["Orange"] = Color.Orange,
            ["Gelb"] = Color.Yellow,
            ["Blau"] = Color.Blue,
            ["Hellblau"] = Color.LightBlue,
            ["Grün"] = Color.Green,
            ["Dunkelgrün"] = Color.DarkGreen,
            ["Braun"] = Color.Brown,
            ["Magenta"] = Color.Magenta,
        };

        public string[] ColorHexStrings
            => colors.Select(kvp => ColorHelper.ToHexString(kvp.Value)).ToArray();

        public string ToName(Color color)
            => colors.FirstOrDefault(c => c.Value.ToArgb() == color.ToArgb()).Key ?? ColorHelper.ToHexString(color);

        public IIndirectBinding<string> ColorBinding
            => Binding.Property<string, string>(c => ToName(ColorHelper.FromHexString(c)));
    }
}
