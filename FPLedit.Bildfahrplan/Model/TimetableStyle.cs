using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.Bildfahrplan.Model
{
    internal class TimetableStyle : Style
    {
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

        public bool[] RenderDays
        {
            get
            {
                var attr = tt.GetAttribute<string>("d") ?? "1111111";
                return DaysHelper.ParseDays(attr);
            }
            set => tt.SetAttribute("d", DaysHelper.DaysToBinString(value));
        }

        //TODO: jTG 3 shV -> int
        public bool StationLines
        {
            get => tt.GetAttribute("shV", true);
            set => tt.SetAttribute("shV", value.ToString().ToLower());
        }

        public bool DrawHeader
        {
            get => tt.GetAttribute("fpl-dh", true);
            set => tt.SetAttribute("fpl-dh", value.ToString().ToLower());
        }

        public int HeightPerHour
        {
            get => tt.GetAttribute("hpH", 150);
            set => tt.SetAttribute("hpH", value.ToString());
        }

        public bool StationVertical
        {
            get => tt.GetAttribute<bool>("sHor", true);
            set => tt.SetAttribute("sHor", value.ToString());
        }

        #region Fonts

        public Font StationFont
        {
            get => ParseFont(tt.GetAttribute<string>("sFont"));
            set => tt.SetAttribute("sFont", FontToString(value));
        }

        public Font TimeFont
        {
            get => ParseFont(tt.GetAttribute<string>("hFont"));
            set => tt.SetAttribute("hFont", FontToString(value));
        }

        public Font TrainFont
        {
            get => ParseFont(tt.GetAttribute<string>("trFont"));
            set => tt.SetAttribute("trFont", FontToString(value));
        }

        #endregion

        #region Colors
        public Color TimeColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-tc"), Color.Orange);
            set => tt.SetAttribute("fpl-tc", ColorToString(value));
        }

        public Color BgColor
        {
            get => ParseColor(tt.GetAttribute<string>("bgC"), Color.White);
            set => tt.SetAttribute("bgC", ColorToString(value));
        }

        public Color StationColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-sc"), Color.Black);
            set => tt.SetAttribute("fpl-sc", ColorToString(value));
        }

        public Color TrainColor
        {
            get => ParseColor(tt.GetAttribute<string>("fpl-trc"), Color.Gray);
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
