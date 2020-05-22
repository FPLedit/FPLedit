using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Model
{
    internal abstract class Style
    {
        internal static IPluginInterface PluginInterface;

        private readonly Timetable parentTimetable;
        protected bool OverrideEntityStyle { get; }

        protected Style(Timetable tt)
        {
            parentTimetable = tt;
            OverrideEntityStyle = PluginInterface.Settings.Get<bool>("bifpl.override-entity-styles");
        }

        protected MColor ParseColor(string def, MColor defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected string ColorToString(MColor color)
            => ColorFormatter.ToString(color, parentTimetable.Version == TimetableVersion.JTG2_x);
    }
}
