using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Timetable : Entity
    {
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

        public List<Station> Stations { get; set; }

        public List<Train> Trains { get; set; }

        public Timetable() : base("jTrainGraph_timetable", null) // Root without parent
        {
            Stations = new List<Station>();
            Trains = new List<Train>();

            SetAttribute("version", "008");
            Children.Add(new XMLEntity("stations"));
            Children.Add(new XMLEntity("trains"));
        }

        public Timetable(XMLEntity en) : base(en, null) // Root without parent
        {
            Stations = new List<Station>();
            foreach(var c in Children.First(x => x.XName == "stations").Children.
                Where(x => x.XName == "sta")) // Filtert andere Elemente
                Stations.Add(new Station(c, this));

            Trains = new List<Train>();
            foreach (var c in Children.First(x => x.XName == "trains").Children.
                Where(x => x.XName == "ti" || x.XName == "ta")) // Filtert andere Elemente
                Trains.Add(new Train(c, this));
        }

        public List<Station> GetStationsOrderedByDirection(TrainDirection direction)
        {
            return (direction.Get() ?
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
    }
}
