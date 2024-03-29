﻿using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System.Globalization;

namespace FPLedit.Bildfahrplan.Model;

internal sealed class TimetableStyle : Style
{
    private readonly Timetable tt;

    public TimetableStyle(Timetable tt)
    {
        this.tt = tt;
    }

    public TimeEntry StartTime
    {
        get
        {
            var time = tt.GetAttribute("tMin", "00:00");
            if (time == "")
                time = "00:00";
            return TimeEntry.Parse(time);
        }
        set => tt.SetAttribute("tMin", value.ToTimeString());
    }

    public TimeEntry EndTime
    {
        get
        {
            var time = tt.GetAttribute("tMax", "24:00");
            if (time == "")
                time = "24:00";
            return TimeEntry.Parse(time);
        }
        set => tt.SetAttribute("tMax", value.ToTimeString());
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
        get => (StationLineStyle)tt.GetAttribute("shV", 0);
        set => tt.SetAttribute("shV", ((int)value).ToString());
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
        get => !tt.GetAttribute("sHor", true);
        set => tt.SetAttribute("sHor", (!value).ToString().ToLower());
    }

    public bool MultiTrack
    {
        get => tt.GetAttribute("shMu", true);
        set => tt.SetAttribute("shMu", value.ToString().ToLower());
    }

    public bool DrawNetworkTrains
    {
        get => tt.GetAttribute("fpl-dnt", true);
        set => tt.SetAttribute("fpl-dnt", value.ToString().ToLower());
    }

    #region Fonts

    public MFont StationFont
    {
        get => MFont.ParseJavaString(tt.GetAttribute<string>("sFont"));
        set => tt.SetAttribute("sFont", value.FontToJavaString());
    }

    public MFont TimeFont
    {
        get => MFont.ParseJavaString(tt.GetAttribute<string>("hFont"));
        set => tt.SetAttribute("hFont", value.FontToJavaString());
    }

    public MFont TrainFont
    {
        get => MFont.ParseJavaString(tt.GetAttribute<string>("trFont"));
        set => tt.SetAttribute("trFont", value.FontToJavaString());
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