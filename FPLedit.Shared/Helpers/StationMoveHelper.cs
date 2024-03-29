using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Helpers;

/// <summary>
/// This class provides a safer interface instead of directly setting <see cref="Station.Positions"/>.
/// </summary>
/// <remarks>
/// <para>This interface is only "safer" as things can still go terribly wrong. See the methods documentation for more warnings.</para>
/// <para>You probably don't want to use the unsafe method. You have been warned.</para>
/// </remarks>
public static class StationMoveHelper
{
    /// <summary>
    /// Checks, if a given station needs an unsafe move operation (see <see cref="PerformUnsafeMove"/>)  to fulfill this move operation.
    /// </summary>
    /// <param name="sta">Station to move.</param>
    /// <param name="existingStation">Is this station already registered at the Timetable instance?</param>
    /// <param name="newPos">New position to move the station to.</param>
    /// <param name="route">Route the move should be applied to.</param>
    /// <remarks>This method is safe to use, as it does not modify state.</remarks>
    /// <returns>true, when an unsafe update is required.</returns>
    private static bool RequiresUnsafeMove(Station sta, bool existingStation, float newPos, int route)
    {
        if (!existingStation)
            return false;

        var tt = sta.ParentTimetable;
        var rt = tt.GetRoute(route).Stations;
        var idx = rt.IndexOf(sta);

        if (!tt.Trains.Any())
            return false; // We don't have to rewrite trains if no trains are existing...

        if (idx == -1)
            return false; //HACK: Not on this route. Probably something weird happening, but at least try to exit gracefully.

        float? min = null, max = null;
        if (idx < rt.Count - 1)
            max = rt[idx + 1].Positions.GetPosition(route);
        if (idx > 0)
            min = rt[idx - 1].Positions.GetPosition(route);

        return ((min.HasValue && newPos < min) || (max.HasValue && newPos > max));
    }

    /// <summary>
    /// Attemps to safely move a station to a new position. It will fail and return false, if this is not possible.
    /// </summary>
    /// <param name="sta">Station to move.</param>
    /// <param name="existingStation">Is this station already registered at the Timetable instance?</param>
    /// <param name="newPos">New position to move the station to.</param>
    /// <param name="route">Route the move should be applied to.</param>
    /// <remarks>
    /// This method is safe to use as it will only to perform a safe move when this is possible.
    /// See <see cref="RequiresUnsafeMove"/> to check beforehand.
    /// </remarks>
    /// <returns>Whether this action was successful. If false, no move was performed, and a unsafe move is required.</returns>
    public static bool TrySafeMove(Station sta, bool existingStation, float newPos, int route)
    {
        var needsUnsafeUpdate = RequiresUnsafeMove(sta, existingStation, newPos, route);

        if (!needsUnsafeUpdate)
        {
            sta.Positions.SetPosition(route, newPos);
            sta.ParentTimetable.RebuildRouteCache(route);
        }

        return !needsUnsafeUpdate;
    }

    /// <summary>
    /// <para>Moves a station to the given new position. But if the new position lies between two other stations, it will try to resort the route's stations AND WILL PROBALY FAIL.</para>
    /// <para>Don't use it if you're unsure. (Better: Never, ever.)</para>
    /// <para>If you want to ask user first if he wants to destroy his data: use <see cref="RequiresUnsafeMove"/>.</para>
    /// <para>If no unsafe update is needed, it just moves the station safely (Nothing bad happened then. Check return value)</para>
    /// </summary>
    /// <param name="sta">Station to move.</param>
    /// <param name="existingStation">Is this station already registered at the Timetable instance? If false, it will just perform a safe move.</param>
    /// <param name="newPos">New position to move the station to.</param>
    /// <param name="route">Route the move should be applied to.</param>
    /// <remarks>
    /// <para>TL;DR: Don't use this.</para>
    /// <para>This method is considered MASSIVELY DANGEROUS.</para>
    /// <para>It WILL DEFINITELY lead to unexpected effects on trains running over this station on this route and possibly other DATA CORRUPTION (immediate and/or later) and/or other SIDE EFFECTS and WILL REQUIRE manual fixing!</para>
    /// <para>GOOD LUCK!</para>
    /// </remarks>
    /// <returns>Allows to check if users data is now at risk: true if we actually did something potentially bad. If false, we just moved the station safely, as we didn't have to do do dangerous things (all went well, no risky things happened).</returns>
    /// <exception cref="InvalidOperationException">Data loss occured while performing this task. Recovery is not possible. We have warned you.</exception>
    public static bool PerformUnsafeMove(Station sta, bool existingStation, float newPos, int route)
    {
        var requiresUnsafeMove = RequiresUnsafeMove(sta, existingStation, newPos, route);

        if (!requiresUnsafeMove)
        {
            sta.Positions.SetPosition(route, newPos);
            sta.ParentTimetable.RebuildRouteCache(route);
            return false;
        }

        var updates = new List<(IWritableTrain train, ArrDep arrdep)>();

        foreach (var tra in sta.ParentTimetable.Trains)
        {
            if (tra is not IWritableTrain wt)
                continue;

            var path = tra.GetPath();
            var idx = path.IndexOf(sta);
            if (idx == -1) // Station not in path; not applicable to train.
                continue;

            // Filter out trains that don't use the current editing session's route id.
            var prev = path.ElementAtOrDefault(idx - 1);
            var next = path.ElementAtOrDefault(idx + 1);

            if (!(prev?.Routes.Contains(route) ?? false) && !(next?.Routes.Contains(route) ?? false))
                continue;

            // This train runs over this station on this route, so we need to update it.
            var arrDep = tra.GetArrDep(sta);
            updates.Add((wt, arrDep.Copy()));
            wt.RemoveArrDep(sta);
        }

        sta.Positions.SetPosition(route, newPos);
        sta.ParentTimetable.RebuildRouteCache(route); // Recreate cache entry, as we will use this in the next block.

        foreach (var u in updates)
        {
            var a = u.train.AddArrDep(sta, route); // this adds the new arrdep to the correct place.
            if (a == null)
                throw new InvalidOperationException("Invalid state: Unexpected data loss while re-writing train: " + u.train.TName);
            a.ApplyCopy(u.arrdep);
        }

        foreach (var rtIdx in sta.Routes)
            sta.ParentTimetable.RebuildRouteCache(rtIdx);

        return true;
    }
}