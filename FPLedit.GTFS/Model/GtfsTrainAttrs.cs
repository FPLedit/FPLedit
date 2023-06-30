using System;
using FPLedit.GTFS.GTFSLib;
using FPLedit.Shared;

namespace FPLedit.GTFS.Model;

public class GtfsTrainAttrs
{
    public ITrain Train { get; }

    public string DaysOverride
    {
        get => Train.GetAttribute("fpl-gtfs-days", "");
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("GTFS attributes of linked train cannot be changed!");
            Train.SetAttribute("fpl-gtfs-days", value);
        }
    }

    public AccessibilityState BikesAllowed
    {
        get => Enum.Parse<AccessibilityState>(Train.GetAttribute("fpl-gtfs-bikes", ((int)AccessibilityState.NotDefined).ToString()));
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("GTFS attributes of linked train cannot be changed!");
            Train.SetAttribute("fpl-gtfs-bikes", ((int) value).ToString());
        }
    }

    public AccessibilityState WheelchairAccessible
    {
        get => Enum.Parse<AccessibilityState>(Train.GetAttribute("fpl-gtfs-wheelchair", ((int)AccessibilityState.NotDefined).ToString()));
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("GTFS attributes of linked train cannot be changed!");
            Train.SetAttribute("fpl-gtfs-wheelchair", ((int) value).ToString());
        }
    }

    public GtfsTrainAttrs(ITrain train)
    {
        Train = train;
    }

    public void ResetDefaults()
    {
        BikesAllowed = AccessibilityState.NotDefined;
        WheelchairAccessible = AccessibilityState.NotDefined;
        DaysOverride = "";
    }
}