using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Timetable : Entity
    {
        XMLEntity sElm, tElm;

        public string TTName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        private List<Station> stations;
        private List<Train> trains;

        public List<Station> Stations => stations;

        public List<Train> Trains => trains;

        public Timetable() : base("jTrainGraph_timetable", null) // Root without parent
        {
            stations = new List<Station>();
            trains = new List<Train>();

            SetAttribute("version", "008");
            sElm = new XMLEntity("stations");
            tElm = new XMLEntity("trains");
            Children.Add(sElm);
            Children.Add(tElm);
        }

        public Timetable(XMLEntity en) : base(en, null) // Root without parent
        {
            stations = new List<Station>();
            sElm = Children.FirstOrDefault(x => x.XName == "stations");
            if (sElm != null)
            {
                foreach (var c in sElm.Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                    stations.Add(new Station(c, this));
            }
            else
            {
                sElm = new XMLEntity("stations");
                Children.Add(sElm);
            }

            trains = new List<Train>();
            tElm = Children.FirstOrDefault(x => x.XName == "trains");
            if (sElm == null && tElm != null)
                throw new Exception("Kein <stations>-Element vorhanden, dafür aber <trains>!");

            if (tElm != null)
            {
                foreach (var c in tElm.Children.Where(x => x.XName == "ti" || x.XName == "ta")) // Filtert andere Elemente
                    trains.Add(new Train(c, this));
            }
            else
            {
                tElm = new XMLEntity("trains");
                Children.Add(tElm);
            }
        }

        public List<Station> GetStationsOrderedByDirection(TrainDirection direction)
        {
            return (direction == TrainDirection.ta ?
                Stations.OrderByDescending(s => s.Kilometre)
                : Stations.OrderBy(s => s.Kilometre)).ToList();
        }

        public string GetLineName(TrainDirection direction)
        {
            var stas = GetStationsOrderedByDirection(direction);
            return stas.First().SName + " - " + stas.Last().SName;
        }

        public Timetable Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (Timetable)formatter.Deserialize(stream);
            }
        }

        #region Hilfsmethoden für andere Entitäten

        public void AddStation(Station sta)
        {
            sta._parent = this;
            stations.Add(sta);
            stations = stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stations.IndexOf(sta); // Index vorläufig ermitteln

            // Es können ja noch andere Nodes in den Children sein.
            if (idx != 0)
            {
                var staBefore = stations[idx - 1];
                idx = sElm.Children.IndexOf(staBefore.XMLEntity) + 1;
            }
            else if (stations.Count > idx + 1)
            {
                var staAfter = stations[idx + 1];
                idx = sElm.Children.IndexOf(staAfter.XMLEntity); // Davor einfügen
            }
            sElm.Children.Insert(idx, sta.XMLEntity);

            // Auch bei allen Zügen hinzufügen
            foreach (var t in Trains)
                t.AddArrDep(sta, new ArrDep());
        }

        public void RemoveStation(Station sta)
        {
            foreach (var train in Trains)
                train.RemoveArrDep(sta);

            var needsCleanup = stations.First() == sta || stations.Last() == sta;

            sta._parent = null;
            stations.Remove(sta);
            sElm.Children.Remove(sta.XMLEntity);

            if (!needsCleanup)
                return;

            // Wenn Endstationen gelöscht werden könnten sonst korrupte Dateien entstehen!
            foreach (var train in Trains)
                train.RemovedOrphanedTimes();
        }

        public void AddTrain(Train tra)
        {
            tra._parent = this;
            trains.Add(tra);
            tElm.Children.Add(tra.XMLEntity);
        }

        public void RemoveTrain(Train tra)
        {
            tra._parent = null;
            trains.Remove(tra);
            tElm.Children.Remove(tra.XMLEntity);
        }

        #endregion

        public string[] GetAllTfzs()
        {
            return Trains
                .Select(t => t.Locomotive)
                .Distinct()
                .Where(s => s != "")
                .ToArray();
        }

        [DebuggerStepThrough]
        public override string ToString()
            => GetLineName(TrainDirection.ta);
    }
}
