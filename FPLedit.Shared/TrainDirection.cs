using System;
using System.Collections.Generic;

namespace FPLedit.Shared;

/// <summary>
/// Specifies the direction of the train.
/// </summary>
/// <remarks>The string represntation of this enum is used as a <see cref="XMLEntity.XName"/> for the train entries.</remarks>
[Templating.TemplateSafe]
public enum TrainDirection
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Trains in line direction (linear timetable only)
    /// </summary>
    ti,

    /// <summary>
    /// Trains against line direction (linear timetable only)
    /// </summary>
    ta,

    /// <summary>
    /// Generic train wkithout specific direction (network timetable only)
    /// </summary>
    tr,
    // ReSharper restore InconsistentNaming
}

[Templating.TemplateSafe(AllowExtensionMethods = true)]
public static class TrainDirectionExt
{
    public static bool IsSortReverse(this TrainDirection dir)
    {
        if (dir == TrainDirection.tr)
            throw new NotSupportedException("TrainDirection tr has no associated sorting behaviour!");
        return dir == TrainDirection.ta;
    }

    public static List<Station> MaybeReverseDirection(this List<Station> stations, TrainDirection dir)
    {
        if (IsSortReverse(dir)) stations.Reverse();
        return stations;
    }
}