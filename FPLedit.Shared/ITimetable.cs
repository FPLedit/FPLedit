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

        int AddRoute(Station s_old, Station s_new, float old_add_km, float new_km);
        Route GetRoute(int index);
        Route[] GetRoutes();
        Station GetStationById(int id);

        void AddTransition(Train first, Train next);
        void SetTransition(Train first, Train newNext);
        Train GetTransition(Train first);
        void RemoveTransition(Train tra, bool onlyAsFirst = true);

        Timetable Clone();
        string[] GetAllTfzs();
        string GetLineName(TrainDirection direction);
        List<Station> GetStationsOrderedByDirection(TrainDirection direction);
    }
}