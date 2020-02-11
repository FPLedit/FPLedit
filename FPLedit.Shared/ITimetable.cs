using System.Collections.Generic;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public interface ITimetable
    {
        TimetableType Type { get; }
        TimetableVersion Version { get; }
        IList<Station> Stations { get; }
        IList<Train> Trains { get; }
        IList<Transition> Transitions { get; }
        string TTName { get; set; }
        int DefaultPrePostTrackTime { get; set; }

        void AddStation(Station sta, int route);
        void AddTrain(Train tra, bool hasArDeps = false);
        Train GetTrainById(int id);
        void RemoveStation(Station sta);
        void RemoveTrain(Train tra);

        bool HasRouteCycles { get; }
        void StationAddRoute(Station sta, int route);
        void StationRemoveRoute(Station sta, int route);
        int AddRoute(Station exisitingStartStation, Station newStation, float newStartPosition, float newPosition);
        void JoinRoutes(int route, Station station, float newKm);
        Route GetRoute(int index);
        Route[] GetRoutes();
        bool RouteConnectsDirectly(int routeToCheck, Station sta1, Station sta2);
        int GetDirectlyConnectingRoute(Station sta1, Station sta2);

        Station GetStationById(int id);

        void AddTransition(Train first, Train next);
        void SetTransition(Train first, Train newNext);
        Train GetTransition(Train first);
        Train GetTransition(int firstTrainId);
        IEnumerable<Train> GetFollowingTransitions(Train first);
        void RemoveTransition(Train tra, bool onlyAsFirst = true);
        void RemoveTransition(int firstTrainId, bool onlyAsFirst = true);
        bool HasTransition(Train tra, bool onlyAsFirst = true);

        Timetable Clone();
        List<Station> GetStationsOrderedByDirection(TrainDirection direction);
    }
}