using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            get
            {
                return GetAttribute<string>("name", "");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        private List<Station> stations;
        private List<Train> trains;

        public List<Station> Stations
        {
            get { return stations; }
        }

        public List<Train> Trains
        {
            get { return trains; }
        }

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
            sElm = Children.First(x => x.XName == "stations");
            foreach (var c in sElm.Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                stations.Add(new Station(c, this));

            trains = new List<Train>();
            tElm = Children.First(x => x.XName == "trains");
            foreach (var c in tElm.Children.Where(x => x.XName == "ti" || x.XName == "ta")) // Filtert andere Elemente
                trains.Add(new Train(c, this));
        }

        public List<Station> GetStationsOrderedByDirection(TrainDirection direction)
        {
            return (direction == TrainDirection.ta ?
                Stations.OrderByDescending(s => s.Kilometre)
                : Stations.OrderBy(s => s.Kilometre)).ToList();
        }

        public string GetLineName(TrainDirection direction)
        {
            string first = GetStationsOrderedByDirection(direction).First().SName;
            string last = GetStationsOrderedByDirection(direction).Last().SName;

            return first + " - " + last;
        }

        public override string ToString()
        {
            return GetLineName(TrainDirection.ta);
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

            sta._parent = null;
            stations.Remove(sta);
            sElm.Children.Remove(sta.XMLEntity);
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

        public string[] GetAllTfzs()
        {
            return Trains
                .Select(t => t.Locomotive)
                .Distinct()
                .Where(s => s != "")
                .ToArray();
        }
    }
}
