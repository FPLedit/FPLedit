using System.Diagnostics.CodeAnalysis;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Model
{
    internal abstract class Style
    {
        public static bool OverrideEntityStyle { get; set; }

        [return: NotNullIfNotNull("defaultValue")]
        protected MColor? ParseColor(string? def, MColor? defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected string ColorToString(MColor color)
            => ColorFormatter.ToString(color);
    }
}
