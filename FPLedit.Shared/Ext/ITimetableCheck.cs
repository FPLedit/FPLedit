using System.Collections.Generic;

namespace FPLedit.Shared;

/// <summary>
/// Registrable action hook that is executed each time the timetable is modified, to provide feedback to the user.
/// </summary>
public interface ITimetableCheck : IRegistrableComponent
{
    /// <summary>
    /// Display name of this timetable check. This might be shwon to the user in the future. It should be rather unique.
    /// </summary>
    string Display { get; }

    /// <summary>
    /// Perform the timetable check.
    /// </summary>
    /// <param name="tt">Read-only copy of the current timetable.</param>
    /// <remarks>This method must be thread-safe and MUST NOT call into UI directly, as it might be called on a non-UI thread.</remarks>
    IEnumerable<TimetableCheckResult> Check(Timetable tt);
}

/// <summary>
/// Result of a <see cref="ITimetableCheck"/> operation.
/// </summary>
public record TimetableCheckResult(string Display);