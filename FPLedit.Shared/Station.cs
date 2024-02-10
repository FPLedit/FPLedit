using System;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared;

/// <summary>
/// This object model type represents a single railway station (with tracks, platforms, etc.) in the network, bound
/// on one or more "Routes". Trains can stop at this type of <see cref="IStation"/>.
/// </summary>
[DebuggerDisplay("{SName} [{GetAttribute(\"km\", \"\")}]")]
[XElmName("sta")]
[Templating.TemplateSafe]
public sealed class Station : Entity, IStation
{
    /// <summary>
    /// Collection that allows to modify the tracks of this station.
    /// </summary>
    public IChildrenCollection<Track> Tracks { get; }

    /// <inheritdoc />
    public Station(XMLEntity en, Timetable tt) : base(en, tt)
    {
        Positions.TestForErrors();
        Tracks = new ObservableChildrenCollection<Track>(this, "track", ParentTimetable);
    }

    /// <summary>
    /// Create a new empty station and associate it with the given timetable.
    /// </summary>
    public Station(Timetable tt) : base("sta", tt)
    {
        Tracks = new ObservableChildrenCollection<Track>(this, "track", ParentTimetable);
    }

    /// <inheritdoc />
    [XAttrName("name")]
    public string SName
    {
        get => GetAttribute("name", "");
        set => SetAttribute("name", value);
    }

    /// <summary>
    /// Optional metadata entry that contains the user-set station code. May be displayed on some outputs.
    /// </summary>
    [XAttrName("fpl-cd", IsFpleditElement = true)]
    public string StationCode
    {
        get => GetAttribute("fpl-cd", "");
        set => SetAttribute("fpl-cd", value);
    }

    /// <summary>
    /// Optional metadata entry that contains the user-set station type. May be displayed on some outputs.
    /// </summary>
    [XAttrName("fpl-tp", IsFpleditElement = true)]
    public string StationType
    {
        get => GetAttribute("fpl-tp", "");
        set => SetAttribute("fpl-tp", value);
    }

    /// <summary>
    /// Optional boolean flag: Whether the train stops always or only on request.
    /// </summary>
    [XAttrName("fpl-rq", IsFpleditElement = true)]
    public bool RequestStop
    {
        get => Convert.ToBoolean(GetAttribute<int>("fpl-rq"));
        set => SetAttribute("fpl-rq", value ? "1" : "0");
    }

    /// <inheritdoc />
    public PositionCollection Positions
        => new(this, ParentTimetable);

    /// <summary>
    /// Track count on the route (not the station), to the right of the station. Depends on route index.
    /// </summary>
    [XAttrName("tr")]
    public RouteValueCollection<int> LineTracksRight
        => new(this, ParentTimetable, "tr", "1", int.Parse, i => i.ToString());

    /// <inheritdoc />
    [XAttrName("fpl-wl", IsFpleditElement = true)]
    public RouteValueCollection<int> Wellenlinien
        => new(this, ParentTimetable, "fpl-wl", "0", int.Parse, i => i.ToString());

    /// <inheritdoc />
    [XAttrName("fpl-vmax", IsFpleditElement = true)]
    public RouteValueCollection<string> Vmax
        => new(this, ParentTimetable, "fpl-vmax", "", s => s, s => s);

    /// <summary>
    /// Deafult track against the route direction. Depends on route index.
    /// </summary>
    /// <remarks>Tracks must be registered beforehand at <see cref="Tracks"/>.</remarks>
    [XAttrName("dTi")]
    public RouteValueCollection<string> DefaultTrackRight
        => new(this, ParentTimetable, "dTi", "", s => s, s => s);

    /// <summary>
    /// Deafult track in the route direction. Depends on route index.
    /// </summary>
    /// <remarks>Tracks must be registered beforehand at <see cref="Tracks"/>.</remarks>
    [XAttrName("dTa")]
    public RouteValueCollection<string> DefaultTrackLeft
        => new(this, ParentTimetable, "dTa", "", s => s, s => s);

    /// <inheritdoc />
    [XAttrName("fpl-id", IsFpleditElement = true)]
    public int Id
    {
        get
        {
            if (ParentTimetable.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
            return GetAttribute<int>("fpl-id");
        }
        internal set
        {
            if (ParentTimetable.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
            if (GetAttribute<string>("fpl-id") != null)
                throw new InvalidOperationException("Station hat bereits eine Id!");
            SetAttribute("fpl-id", value.ToString());
        }
    }

    private int[]? routeCache;

    /// <inheritdoc />
    /// <exception cref="TimetableTypeNotSupportedException">If setting this value on a linear timetable.</exception>
    [XAttrName("fpl-rt", IsFpleditElement = true)]
    public int[] Routes
    {
        get
        {
            if (ParentTimetable.Type == TimetableType.Linear)
                return new[] { 0 };

            if (routeCache == null)
            {
                routeCache = GetAttribute("fpl-rt", "")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s)).ToArray();
            }

            return routeCache;
        }
        private set
        {
            if (ParentTimetable.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "route ids");
            SetAttribute("fpl-rt", string.Join(",", value));
            routeCache = null;
        }
    }

    /// <summary>
    /// Returns if this station connects more than one route.
    /// </summary>
    public bool IsJunction => Routes.Length > 1;

    internal bool _InternalAddRoute(int route)
    {
        if (Routes.Contains(route))
            return false;
        var list = Routes.ToList();
        list.Add(route);
        Routes = list.ToArray();
        return true;
    }

    internal bool _InternalRemoveRoute(int route)
    {
        if (!Routes.Contains(route))
            return false;

        var list = Routes.ToList();
        list.Remove(route);
        Routes = list.ToArray();
        Positions.RemovePosition(route);

        // Remove RVC values.
        foreach (var rvc in GetDefinedRvcs())
            rvc.RemoveValue(route);

        return true;
    }

    internal bool _InternalReplaceRoute(int oldRoute, int newRoute)
    {
        if (!Routes.Contains(oldRoute))
            return false;

        var list = Routes.ToList();
        list.Remove(oldRoute);
        list.Add(newRoute);
        Routes = list.ToArray();

        // Swap positions.
        var oldPosition = Positions.GetPosition(oldRoute);
        Positions.RemovePosition(oldRoute);
        if (oldPosition.HasValue)
            Positions.SetPosition(newRoute, oldPosition.Value);

        // Swap RVC values.
        foreach (var rvc in GetDefinedRvcs())
            rvc.SwapRouteId(oldRoute, newRoute);

        return true;
    }

    private IRouteValueCollection[] GetDefinedRvcs()
    {
        // We do not deal with external RVCs here, see remarks on RouteValueCollction{T}.
        return new IRouteValueCollection[] { Wellenlinien, Vmax, DefaultTrackLeft, DefaultTrackRight, LineTracksRight };
    }
}