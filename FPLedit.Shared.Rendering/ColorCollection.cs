using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    public class ColorCollection
    {
        private Dictionary<string, MColor> colors = new Dictionary<string, MColor>()
        {
            ["Schwarz"] = (MColor)Colors.Black,
            ["Grau"] = (MColor)Colors.Gray,
            ["Weiß"] = (MColor)Colors.White,
            ["Rot"] = (MColor)Colors.Red,
            ["Orange"] = (MColor)Colors.Orange,
            ["Gelb"] = (MColor)Colors.Yellow,
            ["Blau"] = (MColor)Colors.Blue,
            ["Hellblau"] = (MColor)Colors.LightBlue,
            ["Grün"] = (MColor)Colors.Green,
            ["Dunkelgrün"] = (MColor)Colors.DarkGreen,
            ["Braun"] = (MColor)Colors.Brown,
            ["Magenta"] = (MColor)Colors.Magenta,
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
                    colors.Add(parts[0], ColorFormatter.FromHexString(parts[1]));
                }
            }
        }
    }
}
