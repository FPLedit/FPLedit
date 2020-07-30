using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    public sealed class ColorCollection
    {
        private readonly Dictionary<string, MColor> colors = new Dictionary<string, MColor>()
        {
            [T._("Schwarz")] = (MColor)Colors.Black,
            [T._("Grau")] = (MColor)Colors.Gray,
            [T._("Weiß")] = (MColor)Colors.White,
            [T._("Rot")] = (MColor)Colors.Red,
            [T._("Orange")] = (MColor)Colors.Orange,
            [T._("Gelb")] = (MColor)Colors.Yellow,
            [T._("Blau")] = (MColor)Colors.Blue,
            [T._("Hellblau")] = (MColor)Colors.LightBlue,
            [T._("Grün")] = (MColor)Colors.Green,
            [T._("Dunkelgrün")] = (MColor)Colors.DarkGreen,
            [T._("Braun")] = (MColor)Colors.Brown,
            [T._("Magenta")] = (MColor)Colors.Magenta,
        };

        public string[] ColorHexStrings
            => colors.Select(kvp => ColorFormatter.ToString(kvp.Value)).ToArray();

        public string ToName(MColor color)
            => colors.FirstOrDefault(c => c.Value == color).Key ?? ColorFormatter.ToString(color);

        public ColorCollection(ISettings settings)
        {
            var setting = settings.Get<string>("core.colors");
            if (setting != null)
            {
                var customColors = setting.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in customColors)
                {
                    var parts = c.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    var color = ColorFormatter.FromHexString(parts[1]);
                    if (color != null)
                        colors.Add(parts[0], color);
                }
            }
        }
    }
}
