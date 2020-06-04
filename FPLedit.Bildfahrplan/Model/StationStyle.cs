using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Model
{
    internal sealed class StationStyle : Style
    {
        public Station Station { get; }
        
        private readonly TimetableStyle ttStyle;
        public StationStyle(Station sta, TimetableStyle ttStyle) : base(sta.ParentTimetable)
        {
            this.Station = sta;
            this.ttStyle = ttStyle;
        }

        public StationStyle(Station sta) : base(sta.ParentTimetable)
        {
            this.Station = sta;
        }
        
        public void ResetDefaults()
        {
            StationColor = null;
            StationWidth = null;
            LineStyle = 0;
            Show = true;
        }

        public MColor StationColor
        {
            get => ParseColor(Station.GetAttribute<string>("cl"), null);
            set => Station.SetAttribute("cl", ColorToString(value ?? MColor.White));
        }
        public MColor CalcedColor => OverrideEntityStyle ? ttStyle.StationColor :(StationColor ?? ttStyle.StationColor);
        public string HexColor
        {
            get => StationColor != null ? ColorFormatter.ToString(StationColor, false) : null;
            set => StationColor = ColorFormatter.FromString(value, MColor.White);
        }

        public int? StationWidth
        {
            get
            {
                var val = Station.GetAttribute("sz", -1);
                if (val == -1)
                    return null;
                return val;
            }
            set => Station.SetAttribute("sz", value.ToString());
        }
        public int CalcedWidth => OverrideEntityStyle ? ttStyle.StationWidth : (StationWidth ?? ttStyle.StationWidth);
        public int StationWidthInt
        {
            get => Station.GetAttribute("sz", -1);
            set => Station.SetAttribute("sz", value.ToString());
        }

        public bool Show
        {
            get => Station.GetAttribute("sh", true);
            set => Station.SetAttribute("sh", value.ToString().ToLower());
        }
        public bool CalcedShow => OverrideEntityStyle || Show;

        public int LineStyle
        {
            get => Station.GetAttribute("sy", 0);
            set => Station.SetAttribute("sy", value.ToString());
        }
        public int CalcedLineStyle => OverrideEntityStyle ? 0 : LineStyle;
    }
}
