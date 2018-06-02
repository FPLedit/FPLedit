using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan
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
            => colors.Select(kvp => ColorFormatter.ToString(kvp.Value)).ToArray();

        public string ToName(Color color)
            => colors.FirstOrDefault(c => c.Value.ToArgb() == color.ToArgb()).Key ?? ColorFormatter.ToString(color);

        public IIndirectBinding<string> ColorBinding
            => Binding.Property<string, string>(c => ToName(ColorFormatter.FromHexString(c)));

        public ColorCollection(ISettings settings)
        {
            var setting = settings.Get<string>("bifpl.colors");
            if (settings != null)
            {
                var customColors = setting.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in customColors)
                {
                    var parts = c.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    colors.Add(parts[0], ColorFormatter.FromHexString(parts[1]));
                }
            }
        }
    }
}
