namespace FPLedit.Shared;

/// <summary>
/// Contains information about the compatibility of a given file version with FPLedit as well as jTrainGraph.
/// </summary>
[Templating.TemplateSafe]
public record TimetableVersionCompat(TimetableVersion Version, TtVersionCompatType Compatibility, TimetableType Type, (string version, TtVersionJtgCompat cp)[] JtgVersionCompatibility);

/// <summary>
/// Specifies, whether a given file version is compatible with FPLedit.
/// </summary>
[Templating.TemplateSafe]
public enum TtVersionCompatType
{
    /// <summary>
    /// FPLedit can't open this file version (any more).
    /// </summary>
    None,
    /// <summary>
    /// FPLedit can read the file, but can only upgrade it to a newer file version.
    /// </summary>
    UpgradeOnly,
    /// <summary>
    /// FPLedit fully supports this file version.
    /// </summary>
    ReadWrite,
}

/// <summary>
/// Specifies, whether a given file version is compatible with jTrainGraph.
/// </summary>
/// <remarks>There is no value for encoding "No compatibility", as this can be achieved by just omitting the compatibility entry.</remarks>
[Templating.TemplateSafe]
public enum TtVersionJtgCompat
{
    /// <summary>
    /// jTrainGraph only supports opening linear files of this version.
    /// </summary>
    OnlyLinear,
    /// <summary>
    /// Currently unused (for future versions of jTrainGraph that support network timetables)
    /// </summary>
    FullWithNetwork,
}