﻿using System.Collections.Generic;

namespace FPLedit.Shared
{
    //TODO: Neue Methoden vom Timetable hier eintragen
    public interface ITimetable
    {
        TimetableType Type { get; }
        List<Station> Stations { get; }
        List<Train> Trains { get; }
        string TTName { get; set; }

        void AddStation(Station sta, int route);
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