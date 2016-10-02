using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Shared
{
    [Serializable]
    public sealed class Timetable : Meta
    {
        public const string VERSION = "1.0";

        public string Name { get; set; }

        public List<Station> Stations { get; set; }

        public List<Train> Trains { get; set; }        

        public Timetable() : base()
        {
            Stations = new List<Station>();
            Trains = new List<Train>();

            Metadata["Version"] = VERSION;
        }

        public static Timetable GenerateTestTimetable()
        {
            Timetable t = new Timetable();
            t.Name = "ATal - BTal";
            Station a = new Station() { Name = "ATal", Kilometre = 0.0f, MaxVelocity = 60 };
            Station b = new Station() { Name = "BTal", Kilometre = 1.0f, MaxVelocity = 100 };
            Train tr = new Train() { Name = "P 01", Locomotive = "211" };
            tr.Line = "ATal - BTal";
            tr.Direction = true;
            tr.Arrivals = new Dictionary<Station, TimeSpan>();
            tr.Departures = new Dictionary<Station, TimeSpan>();
            tr.Arrivals.Add(b, DateTime.Now.TimeOfDay);
            tr.Departures.Add(a, DateTime.Now.TimeOfDay);

            Train t2 = new Train() { Name = "P 01", Locomotive = "211" };
            t2.Arrivals = new Dictionary<Station, TimeSpan>();
            t2.Departures = new Dictionary<Station, TimeSpan>();
            t2.Arrivals.Add(b, DateTime.Now.TimeOfDay);
            t2.Departures.Add(a, DateTime.Now.TimeOfDay);

            t.Stations = new List<Station>();
            t.Trains = new List<Train>();
            t.Stations.Add(b);
            t.Stations.Add(a);
            t.Trains.Add(tr);
            t.Trains.Add(t2);

            return t;
        }

        public void SaveToFile(string filename)
        {            
            using (FileStream stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, this);
                }
                catch (SerializationException e)
                {
                    throw new Exception("Ein Fehler ist beim Speichern der Datei aufgetreten: " + e.Message);
                }
            }
        }

        public static Timetable OpenFromFile(string filename)
        {
            Timetable tt;

            using (FileStream stream = File.Open(filename, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {                    
                    tt = (Timetable)formatter.Deserialize(stream);
                }
                catch (SerializationException e)
                {
                    throw new Exception("Ein Fehler ist beim Öffnen der Datei aufgetreten: " + e.Message);
                }
            }

            if (tt.GetMeta("Version", "") != VERSION)
                throw new Exception("Ein Fehler ist beim Öffnen der Datei aufgetreten: Falsche Dateiversion");

            return tt;
        }

        public List<Station> GetStationsOrderedByDirection(bool direction)
        {
            return (direction ?
                Stations.OrderByDescending(s => s.Kilometre)
                : Stations.OrderBy(s => s.Kilometre)).ToList();
        }

        public string GetLineName(bool direction)
        {
            string first = GetStationsOrderedByDirection(direction).First().Name;
            string last = GetStationsOrderedByDirection(direction).Last().Name;

            return first + " - " + last;
        }

        public override string ToString()
        {
            return GetLineName(true);
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
