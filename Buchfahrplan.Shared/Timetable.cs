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
    public sealed class Timetable
    {
        public string Name { get; set; }

        public List<Station> Stations { get; set; }

        public List<Train> Trains { get; set; }

        public Timetable()
        {
            Stations = new List<Station>();
            Trains = new List<Train>();
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
            tr.Arrivals = new Dictionary<Station, DateTime>();
            tr.Departures = new Dictionary<Station, DateTime>();
            tr.Arrivals.Add(b, DateTime.Now);
            tr.Departures.Add(a, DateTime.Now);

            Train t2 = new Train() { Name = "P 01", Locomotive = "211" };
            t2.Arrivals = new Dictionary<Station, DateTime>();
            t2.Departures = new Dictionary<Station, DateTime>();
            t2.Arrivals.Add(b, DateTime.Now);
            t2.Departures.Add(a, DateTime.Now);

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
            FileStream fs = new FileStream(filename, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
            }
            catch (SerializationException e)
            {
                throw new Exception("Ein Fehler ist beim Speichern der Datei aufgetreten: " + e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        public static Timetable OpenFromFile(string filename)
        {
            Timetable tt;
            
            FileStream fs = new FileStream(filename, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                tt = (Timetable)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                throw new Exception("Ein Fehler ist beim Öffnen der Datei aufgetreten: " + e.Message);
            }
            finally
            {
                fs.Close();
            }

            return tt;
        }

        public string GetLineName(bool direction)
        {
            string first = Stations.First().Name;
            string last = Stations.Last().Name;          

            if (!direction)
                return first + " - " + last;
            else
                return last + " - " + first;
        }

        public override string ToString()
        {
            return GetLineName(true);
        }
    }    
}
