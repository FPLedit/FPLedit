using System.Collections.Generic;

namespace FPLedit.Shared
{
    public interface ITimetable
    {
        List<Station> Stations { get; }
        List<Train> Trains { get; }
        string TTName { get; set; }

        void AddStation(Station sta);
        void AddTrain(Train tra, bool hasArDeps = false);
        Timetable Clone();
        string[] GetAllTfzs();
        string GetLineName(TrainDirection direction);
        List<Station> GetStationsOrderedByDirection(TrainDirection direction);
        void RemoveStation(Station sta);
        void RemoveTrain(Train tra);
        string ToString();
    }
}