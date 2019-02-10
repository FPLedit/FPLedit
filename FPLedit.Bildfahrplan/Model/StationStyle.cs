using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Bildfahrplan.Model
{
    internal class StationStyle : Style
    {
        private Station sta;
        private TimetableStyle ttStyle;
        public StationStyle(Station sta, TimetableStyle ttStyle) : base(sta._parent)
        {
            this.sta = sta;
            this.ttStyle = ttStyle;
        }

        public StationStyle(Station sta) : base(sta._parent)
        {
            this.sta = sta;
        }

        public MColor StationColor
        {
            get => ParseColor(sta.GetAttribute<string>("cl"), null);
            set => sta.SetAttribute("cl", ColorToString(value ?? MColor.White));
        }
        public MColor CalcedColor => overrideEntityStyle ? ttStyle.StationColor :(StationColor ?? ttStyle.StationColor);

        public int? StationWidth
        {
            get
            {
                var val = sta.GetAttribute("sz", -1);
                if (val == -1)
                    return null;
                return val;
            }
            set => sta.SetAttribute("sz", value.ToString());
        }
        public int CalcedWidth => overrideEntityStyle ? ttStyle.StationWidth : (StationWidth ?? ttStyle.StationWidth);

        public bool Show
        {
            get => sta.GetAttribute("sh", true);
            set => sta.SetAttribute("sh", value.ToString().ToLower());
        }
        public bool CalcedShow => overrideEntityStyle || Show;

        public int LineStyle
        {
            get => sta.GetAttribute("sy", 0);
            set => sta.SetAttribute("sy", value.ToString());
        }
        public int CalcedLineStyle => overrideEntityStyle ? 0 : LineStyle;
    }
}
