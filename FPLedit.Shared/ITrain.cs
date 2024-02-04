using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FPLedit.Shared;

/// <summary>
/// Object model representing a single train of this timetable.
/// </summary>
[Templating.TemplateSafe]
public interface ITrain : IEntity
{
    /// <summary>
    /// The (not necessarily unique) identifier of this train.
    /// </summary>
    /// <remarks>Train IDs may be shared by multiple linked trains.</remarks>
    int Id { get; }
    /// <summary>
    /// The unique identifier of this train, suitable for referencing this train in other places.
    /// </summary>
    string QualifiedId { get; }
    /// <summary>
    /// Retuns the name of this train.
    /// </summary>
    string TName { get; }
    /// <summary>
    /// Optional comment. May be shown in some output formats.
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// Specifies whether this train is linked to a base train.
    /// </summary>
    /// <remarks>Linked trains are currentlxy not supported by FPLedit.</remarks>
    bool IsLink { get; }

    /// <summary>
    /// All weekdays this train is running.
    /// </summary>
    Days Days { get; }
    /// <summary>
    /// Retrieves the train's direction according to the XML element name.
    /// </summary>
    /// <remarks>In Network timetables this will always return the generic <see cref="TrainDirection.tr"/>, so cannot be used to determine the real direction.</remarks>
    TrainDirection Direction { get; }
    /// <summary>
    /// Optional metadata entry that contains the uses-supplied weight of this train. May be shown in some output formats.
    /// </summary>
    string Last { get; }
    /// <summary>
    /// Optional metadata entry that contains the uses-supplied locmotive tractioning this train. May be shown in some output formats.
    /// </summary>
    string Locomotive { get; }
    /// <summary>
    /// Optional metadata entry that contains the uses-supplied minimum brake percent of this train. May be shown in some output formats.
    /// </summary>
    string Mbr { get; }

    /// <summary>
    /// Returns the full path of this train, ordered by the running direction. In linear timetables it returns the full line, ordered in the running direction of the train.
    /// </summary>
    List<Station> GetPath();
    /// <summary>
    /// Returns the time entry at the given station, throws if not found.
    /// </summary>
    ArrDep GetArrDep(Station sta);
    /// <summary>
    /// Tries to retrieve the time entry at the given station, but will not throw if it is not found.
    /// </summary>
    /// <param name="sta">The station to look for as atime entry.</param>
    /// <param name="arrDep">Out parameter containing the found time entry, or null if the return value is false.</param>
    /// <returns>true, if the time entry was found.</returns>
    bool TryGetArrDep(Station sta, [NotNullWhen(returnValue: true)] out ArrDep? arrDep);
    /// <summary>
    /// This functions return value contains all <see cref="ArrDep"/>s of this train, but sorting may not be preserved. (It is a dictionary!) Use <see cref="GetPath"/> to get the correct sorted stations.
    /// </summary>
    /// <returns></returns>
    Dictionary<Station, ArrDep> GetArrDepsUnsorted();

    /// <summary>
    /// Get a display name for the path of the train.
    /// </summary>
    string GetLineName();
}

/// <summary>
/// Object model, representing a single train, that can be edited.
/// </summary>
public interface IWritableTrain : ITrain
{
    /// <summary>
    /// The unique identifier of this train.
    /// </summary>
    new int Id { get; set; }
    /// <summary>
    /// Retuns the name of this train.
    /// </summary>
    new string TName { get; set; }
    /// <summary>
    /// Optional comment. May be shown in some output formats.
    /// </summary>
    new string Comment { get; set; }

    /// <summary>
    /// Add a new time entry for all stations in the gievn path. This operation is only applicable to Network Timetables.
    /// </summary>
    /// <remarks>This can only be applied when the train currently has no time entries.</remarks>
    void AddAllArrDeps(IEnumerable<Station> path);
    /// <summary>
    /// Prepares the train for use in a linear timetable and adds all station entries ordered in line direction.
    /// </summary>
    /// <remarks>This can only be applied when the train currently has no time entries.</remarks>
    void AddLinearArrDeps();
    /// <summary>
    /// Inserts the given time entry for the station added after creation of the train.
    /// </summary>
    /// <param name="sta"></param>
    /// <param name="route">The route index, at which the station is used, or <see cref="Timetable.LINEAR_ROUTE_ID"/> if the timetable is linear.</param>
    /// <returns>The newly added time entry.</returns>
    ArrDep? AddArrDep(Station sta, int route);
    /// <summary>
    /// Removes the time entry of the given station. May require removing the station from the timetable altogether, to keep the timetable consistent.
    /// </summary>
    void RemoveArrDep(Station sta);
    /// <summary>
    /// Cleans up orphaned time entries after deleting stations (e.g. arrival time at the first station)
    /// </summary>
    void RemoveOrphanedTimes();

    /// <summary>
    /// All linked trains that are linked to this writable parent train.
    /// </summary>
    TrainLink[] TrainLinks { get; }

    /// <summary>
    /// Add the given link to this train.
    /// </summary>
    void AddLink(TrainLink link);
}