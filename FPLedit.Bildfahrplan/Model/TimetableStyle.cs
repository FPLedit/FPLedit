using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Bildfahrplan.Model
{
    internal class TimetableStyle : Style
    {
        public bool MultiTrack { get; internal set; } //TODO: Read from xml

        private Timetable tt;

        public TimetableStyle(Timetable tt) : base(tt)
        {
            this.tt = tt;
        }

        public TimeSpan StartTime
        {
            get
            {
                int minutes = tt.GetAttribute<int>("tMin", -1);
                if (minutes == -1)
                    minutes = 0;
                return new TimeSpan(0, minutes, 0);
            }
            set
            {
                int minutes = value.GetMinutes();
                tt.SetAttribute("tMin", minutes.ToString());
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                int minutes = tt.GetAttribute<int>("tMax", -1);
                if (minutes == -1)
                    minutes = 1440;
                return new TimeSpan(0, minutes, 0);
            }
            set
            {
                int minutes = value.GetMinutes();
                tt.SetAttribute("tMax", minutes.ToString());
            }
        }

        public bool DisplayKilometre
        {
            get => tt.GetAttribute("sKm", true);
            set => tt.SetAttribute("sKm", value.ToString().ToLower());
        }

        public Days RenderDays
        {
            get
            {
                var attr = tt.GetAttribute<string>("d") ?? "1111111";
                return Days.Parse(attr);
            }
            set => tt.SetAttribute("d", value.ToBinString());
        }

        public StationLineStyle StationLines
        {
            get
            {
                if (tt.Version == TimetableVersion.JTG2_x)
                    return tt.GetAttribute("shV", true) ? StationLineStyle.Normal : StationLineStyle.None;
                else
                    return (StationLineStyle)tt.GetAttribute("shV", 0);
            }
            set
            {
                if (tt.Version == TimetableVersion.JTG2_x)
                    tt.SetAttribute("shV", (value != StationLineStyle.None).ToString().ToLower());
                else
                    tt.SetAttribute("shV", ((int)value).ToString());
            }
        }

        public bool DrawHeader
        {
            get => tt.GetAttribute("fpl-dh", true);
            set => tt.SetAttribute("fpl-dh", value.ToString().ToLower());
        }

        public float HeightPerHour
        {
            get => tt.GetAttribute("hpH", 150f);
            set => tt.SetAttribute("hpH", value.ToString("0.0", CultureInfo.InvariantCulture));
        }

        public bool StationVertical
        {
            get => tt.GetAttribute<bool>("sHor", true);
            set => tt.SetAttribute("sHor", value.ToString());
        }

        #region Fonts

        public MFont StationFont
        {
            get => MFont.Parse(tt.GetAttribute<string>("sFont"));
            set => tt.SetAttribute("sFont", value.FontToString());
        }

        public MFont TimeFont
        {
            get => MFont.Parse(tt.GetAttribute<string>("hFont"));
            set => tt.SetAttribute("hFont", value.FontToString());
        }

        public MFont TrainFont
        {
            get => MFont.Parse(tt.GetAttribute<string>("trFont"));
            set => tt.SetAttribute("trFont", value.FontToString());
        }

        #endregion

        #region Colors
        public MColor TimeColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-tc"), (MColor)Eto.Drawing.Colors.Orange);
            set => tt.SetAttribute("fpl-tc", ColorToString(value));
        }

        public MColor BgColor
        {
            get => ParseColor(tt.GetAttribute<string>("bgC"), (MColor)Eto.Drawing.Colors.White);
            set => tt.SetAttribute("bgC", ColorToString(value));
        }

        public MColor StationColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-sc"), (MColor)Eto.Drawing.Colors.Black);
            set => tt.SetAttribute("fpl-sc", ColorToString(value));
        }

        public MColor TrainColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-trc"), (MColor)Eto.Drawing.Colors.Gray);
            set => tt.SetAttribute("fpl-trc", ColorToString(value));
        }
        #endregion

        #region Thickness
        public int TrainWidth
        {
            get => tt.GetAttribute("fpl-tw", 1);
            set => tt.SetAttribute("fpl-tw", value.ToString());
        }

        public int StationWidth
        {
            get => tt.GetAttribute("fpl-sw", 1);
            set => tt.SetAttribute("fpl-sw", value.ToString());
        }

        public int HourTimeWidth
        {
            get => tt.GetAttribute("fpl-hw", 2);
            set => tt.SetAttribute("fpl-hw", value.ToString());
        }

        public int MinuteTimeWidth
        {
            get => tt.GetAttribute("fpl-mw", 1);
            set => tt.SetAttribute("fpl-mw", value.ToString());
        }
        #endregion
    }
}
