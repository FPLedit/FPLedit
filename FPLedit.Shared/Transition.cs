﻿using System.Diagnostics;

namespace FPLedit.Shared;

/// <summary>
/// Object-model type to contain a single trainsition between to trains ("first" and "next"). This may result in different output.
/// </summary>
/// <remarks>A transition means that one train is run with the same locomotive, staff, cars or similar, but this is not enforced by FPLedit.</remarks>
[DebuggerDisplay("From {First} to {Next}")]
[XElmName("tra", ParentElements = new []{ "transitions" })]
[Templating.TemplateSafe]
public sealed class Transition : Entity
{
    public Transition(Timetable tt) : base("tra", tt)
    {
    }

    public Transition(XMLEntity en, Timetable tt) : base(en, tt)
    {
    }

    [XAttrName("first")]
    public string First
    {
        get => GetAttribute("first", "");
        set => SetAttribute("first", value);
    }

    [XAttrName("next")]
    public string Next
    {
        get => GetAttribute("next", "");
        set => SetAttribute("next", value);
    }

    [XAttrName("df")]
    public Days Days
    {
        get => Days.Parse(GetAttribute("df", "1111111"));
        set => SetAttribute("df", value.ToBinString());
    }

    public const string LAST_STATION = "LAST";

    [XAttrName("staId")]
    public string StationId
    {
        get => GetAttribute("staId", "");
        set => SetAttribute("staId", value);
    }

    public bool IsTransitionValidAt(Station sta)
    {
        if (StationId == LAST_STATION)
            return true; // This is a "wildcard match"

        int id;
        if (ParentTimetable.Type == TimetableType.Linear)
            id = ParentTimetable.GetRoute(Timetable.LINEAR_ROUTE_ID).IndexOf(sta);
        else
            id = sta.Id;

        return StationId == id.ToString();
    }
}

[Templating.TemplateSafe]
public sealed record TransitionEntry(ITrain NextTrain, Days Days, Station? Station);