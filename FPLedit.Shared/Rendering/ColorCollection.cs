using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    public class ColorCollection
    {
        private Dictionary<string, MColor> colors = new Dictionary<string, MColor>()
        {
            ["Schwarz"] = (MColor)Color.Black,
            ["Grau"] = (MColor)Color.Gray,
            ["Weiß"] = (MColor)Color.White,
            ["Rot"] = (MColor)Color.Red,
            ["Orange"] = (MColor)Color.Orange,
            ["Gelb"] = (MColor)Color.Yellow,
            ["Blau"] = (MColor)Color.Blue,
            ["Hellblau"] = (MColor)Color.LightBlue,
            ["Grün"] = (MColor)Color.Green,
            ["Dunkelgrün"] = (MColor)Color.DarkGreen,
            ["Braun"] = (MColor)Color.Brown,
            ["Magenta"] = (MColor)Color.Magenta,
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
