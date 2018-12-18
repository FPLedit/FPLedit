using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using FPLedit.Shared.Helpers;

namespace FPLedit.Bildfahrplan.Model
{
    internal abstract class Style
    {
        internal static IInfo info;

        private Timetable _parent;

        protected readonly bool overrideEntityStyle;

        public Style(Timetable tt)
        {
            _parent = tt;
            overrideEntityStyle = info.Settings.Get<bool>("bifpl.override-entity-styles");
        }

        protected Color? ParseColor(string def, Color? defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected Color ParseColor(string def, Color defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected string ColorToString(Color color)
            => ColorFormatter.ToString(color, _parent.Version == TimetableVersion.JTG2_x);
    }
}
