using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared;

/// <summary>
/// A PositionCollection (PosCol) allows to define position (chainage) attributes which allow for different values
/// on each individual route.
/// </summary>
[Templating.TemplateSafe]
public class PositionCollection
{
    private readonly IStation sta;
    private readonly Dictionary<int, float> positions;
    private readonly Timetable tt;

    /// <summary>
    /// Creates a new PosColl.
    /// </summary>
    /// <param name="s">The station this PC should operate on.</param>
    /// <param name="tt">The parent timetable of the station <paramref name="s"/>.</param>
    public PositionCollection(IStation s, Timetable tt)
    {
        sta = s;
        positions = new Dictionary<int, float>();
        this.tt = tt;
        if (tt.Type == TimetableType.Linear)
            ParseLinear();
        else
            ParseNetwork();
    }

    /// <summary>
    /// This method does nothing, but can be used to test for errors if the PosColl is constructed on-demand.
    /// </summary>
    public void TestForErrors()
    {
    }

    /// <summary>
    /// Returns the position - or null - on the given route.
    /// </summary>
    public float? GetPosition(int route)
    {
        if (positions.TryGetValue(route, out float val))
            return val;
        return null;
    }

    /// <summary>
    /// <para>Directly sets position on a given route.</para>
    /// <para>THIS IS POTENTIALLY DANGEROUS AND COULD MESS UP THE TIMETABLE IF APPLIED ON A REGISTERED STATION, as it does not update train ArrDep entry order.</para>
    /// <para>Use <see cref="StationMoveHelper"/> as a safe(r) replacement for moving Station.</para>
    /// 
    /// <para>This is probably safe when you generate timetable files from scratch, or use it on custom non-<see cref="Station"/> <see cref="IStation"/>-Entity types, as long as they are not entangled with trains.</para>
    /// </summary>
    /// <remarks>
    /// <para>If applied on existing stations: <see cref="StationMoveHelper.PerformUnsafeMove"/> on why this is even worse than that method (and what could possibly go wrong).</para>
    /// <para>It might be neccessary to call <see cref="Timetable.RebuildRouteCache"/> after changing a position.</para>
    /// </remarks>
    /// <param name="route"></param>
    /// <param name="km"></param>
    public void SetPosition(int route, float km)
    {
        positions[route] = km;
        Write();
    }
        
    /// <summary>
    /// Removes the given route position entry from this station.
    /// </summary>
    /// <remarks>USE WITH CARE!</remarks>
    /// <param name="route"></param>
    public void RemovePosition(int route)
    {
        if (tt.Type == TimetableType.Linear && route == Timetable.LINEAR_ROUTE_ID)
            throw new Exception("Removing linear route is not possible!");
        positions.Remove(route);
        Write();
    }


    /// <summary>
    /// Parse the position data data from a multi-route context (e.g. Network timetable).
    /// </summary>
    private void ParseNetwork()
    {
        var toParse = sta.GetAttribute("km", ""); // Format EXTENDED_FPL, km ist gut
        var pos = toParse.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in pos)
        {
            var parts = p.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            positions.Add(int.Parse(parts[0]),
                float.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Parse the position data if there is only one route and we are not in a multi-route context (e.g. linear timetable).
    /// </summary>
    private void ParseLinear()
    {
        var kml = sta.GetAttribute("kml", "0.0");
        var kmr = sta.GetAttribute("kmr", "0.0");
        if (kml != kmr)
            throw new NotSupportedException("Unterschiedliche kmr/kml werden aktuell von FPLedit nicht unterstützt!");
        positions.Add(Timetable.LINEAR_ROUTE_ID, float.Parse(kml, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Write all positions back to the XML attribute.
    /// </summary>
    /// <param name="forceType">Force either network or linear mode (only to be used by conversions!).</param>
    public void Write(TimetableType? forceType = null)
    {
        var t = forceType ?? tt.Type;
        if (t == TimetableType.Linear)
        {
            var posFloat = GetPosition(Timetable.LINEAR_ROUTE_ID) ?? throw new Exception("No linear position found while attempting to write linear positions.");
            var pos = posFloat.ToString("0.0", CultureInfo.InvariantCulture);
            sta.SetAttribute("kml", pos);
            sta.SetAttribute("kmr", pos);
            sta.RemoveAttribute("km");
        }
        else
        {
            var posStrings = positions.Select(kvp => kvp.Key.ToString() + ":" + kvp.Value.ToString("0.0", CultureInfo.InvariantCulture));
            var text = string.Join(";", posStrings);
            sta.SetAttribute("km", text);
            sta.RemoveAttribute("kml");
            sta.RemoveAttribute("kmr");
        }
    }
}