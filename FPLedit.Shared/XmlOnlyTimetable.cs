using System;
using System.Collections.Generic;

namespace FPLedit.Shared;

/// <summary>
/// This is a special, very simplistic type of timetable. It just allows access to <see cref="XmlOnlyTimetable.XMLEntity"/>,
/// <see cref="XmlOnlyTimetable.Version"/> and <see cref="XmlOnlyTimetable.Type"/>. All other properties or methods will throw.
/// </summary>
/// <remarks>If applicable, a fully featured <see cref="Timetable"/> could be constructed – but note the timetable compat checks.</remarks>
public class XmlOnlyTimetable : Entity, ITimetable
{
    private static string ThrowMessage => T._("Nicht schreibbare Dateiversion.");

    public XmlOnlyTimetable(XMLEntity en) : base(en, null)
    {
    }

    public event EventHandler? TrainsXmlCollectionChanged;
    
    private TimetableType? typeCache;

    [XAttrName("version")]
    public TimetableVersion Version => (TimetableVersion) GetAttribute("version", 0);

    public TimetableType Type => typeCache ??= Version.GetVersionCompat().Type;
    
    public IList<Station> Stations => throw new NotSupportedException(ThrowMessage);
    public IList<ITrain> Trains => throw new NotSupportedException(ThrowMessage);
    public IList<Transition> Transitions => throw new NotSupportedException(ThrowMessage);
    public string TTName
    {
        get => throw new NotSupportedException(ThrowMessage);
        set => throw new NotSupportedException(ThrowMessage);
    }

    public TimeEntry DefaultPrePostTrackTime
    {
        get => throw new NotSupportedException(ThrowMessage);
        set => throw new NotSupportedException(ThrowMessage);
    }
    public void AddStation(Station sta, int route) => throw new NotSupportedException(ThrowMessage);

    public Station? GetStationById(int id) => throw new NotSupportedException(ThrowMessage);
    public void RemoveStation(Station sta) => throw new NotSupportedException(ThrowMessage);
    public void AddTrain(ITrain tra) => throw new NotSupportedException(ThrowMessage);
    public ITrain? GetTrainById(int id) => throw new NotSupportedException(ThrowMessage);
    public ITrain? GetTrainByQualifiedId(string id) => throw new NotSupportedException(ThrowMessage);
    public void RemoveTrain(ITrain tra) => throw new NotSupportedException(ThrowMessage);
    public bool HasRouteCycles => throw new NotSupportedException(ThrowMessage);
    public void StationAddRoute(Station sta, int route) => throw new NotSupportedException(ThrowMessage);
    public void StationRemoveRoute(Station sta, int route) => throw new NotSupportedException(ThrowMessage);
    public int AddRoute(Station exisitingStartStation, Station newStation, float newStartPosition, float newPosition) => throw new NotSupportedException(ThrowMessage);
    public bool JoinRoutes(int route, Station station, float newKm) => throw new NotSupportedException(ThrowMessage);
    public (bool success, string? failReason, bool? isSafeFailure) BreakRouteUnsafe(Station station1, Station station2) => throw new NotSupportedException(ThrowMessage);
    public Route GetRoute(int index) => throw new NotSupportedException(ThrowMessage);
    public Route[] GetRoutes() => throw new NotSupportedException(ThrowMessage);
    public bool RouteConnectsDirectly(int routeToCheck, Station sta1, Station sta2) => throw new NotSupportedException(ThrowMessage);
    public int GetDirectlyConnectingRoute(Station sta1, Station sta2) => throw new NotSupportedException(ThrowMessage);
    public bool WouldProduceAmbiguousRoute(Station toDelete) => throw new NotSupportedException(ThrowMessage);
    public void SetTransitions(ITrain first, IEnumerable<TransitionEntry> trans) => throw new NotSupportedException(ThrowMessage);
    public ITrain? GetTransition(ITrain? first, Days? daysFilter = null, Station? stationFilter = null) => throw new NotSupportedException(ThrowMessage);
    public void RemoveTransition(ITrain first, bool onlyAsFirst = true) => throw new NotSupportedException(ThrowMessage);
    public void RemoveTransition(string firstQualifiedTrainId, bool onlyAsFirst = true) => throw new NotSupportedException(ThrowMessage);
    public bool HasTransition(ITrain tra, bool onlyAsFirst = true) => throw new NotSupportedException(ThrowMessage);
    public Timetable Clone() => throw new NotSupportedException(ThrowMessage);
    public void RemoveLink(TrainLink link) => throw new NotSupportedException(ThrowMessage);
}