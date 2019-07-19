using System.Collections.Generic;

namespace FPLedit.Shared
{
    public interface ITimetable
    {
        TimetableType Type { get; }
        TimetableVersion Version { get; }
        List<Station> Stations { get; }
        List<Train> Trains { get; }
        string TTName { get; set; }

        void AddStation(Station sta, int route);
        void AddTrain(Train tra, bool hasArDeps = false);
        Train GetTrainById(int id);
        void RemoveStation(Station sta);
        void RemoveTrain(Train tra);

        int AddRoute(Station exisitingStartStation, Station newStation, float newStartPosition, float newPosition);
        Route GetRoute(int index);
        Route[] GetRoutes();
        Station GetStationById(int id);

        void AddTransition(Train first, Train next);
        void SetTransition(Train first, Train newNext);
        Train GetTransition(Train first);
        IEnumerable<Train> GetTransitions(Train first);
        void RemoveTransition(Train tra, bool onlyAsFirst = true);

        Timetable Clone();
        List<Station> GetStationsOrderedByDirection(TrainDirection direction);
    }
}