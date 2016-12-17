using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Standard
{
    public class BfplImport : IImport
    {
        public string Filter
        {
            get
            {
                return "FPLedit Dateien (*.bfpl)|*.bfpl";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                using (FileStream stream = File.Open(filename, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                        return DeserializeTimetable(reader);
                }
            }
            catch (Exception ex)
            {
                logger.Error("BfplImport: " + ex.Message);
                return null;
            }
        }

        public Timetable DeserializeTimetable(BinaryReader reader)
        {
            Timetable res = new Timetable();

            string magic = reader.ReadString();
            if (magic != Timetable.MAGIC)
                throw new Exception("Ein Fehler ist beim Öffnen der Datei aufgetreten: Falsche Dateiversion");

            var name = reader.ReadString();
            res.Attributes = DeserializeAttributes(reader);
            res.Name = name;

            var stations = new Dictionary<int, Station>();
            int sta_count = reader.ReadInt32();
            for (int i = 0; i < sta_count; i++)
            {
                int id = reader.ReadInt32();
                var sta = DeserializeStation(reader);
                stations.Add(id, sta);
                res.Stations.Add(sta);
            }

            int tra_count = reader.ReadInt32();
            for (int i = 0; i < tra_count; i++)
            {
                var tr = DeserializeTrain(reader, stations);
                res.Trains.Add(tr);
            }

            return res;
        }

        public Train DeserializeTrain(BinaryReader reader, Dictionary<int, Station> stations)
        {
            Train res = new Train();
            var name = reader.ReadString();
            var tfz = reader.ReadString();
            res.Direction = reader.ReadBoolean() ? TrainDirection.ta : TrainDirection.ti;
            res.Line = reader.ReadString();
            var days = Train.ParseDays(reader.ReadString());
            res.Attributes = DeserializeAttributes(reader);
            res.Name = name;
            res.Days = days;
            res.Locomotive = tfz;

            int arr_count = reader.ReadInt32();
            for (int i = 0; i < arr_count; i++)
            {
                int sta_id = reader.ReadInt32();
                Station sta = stations[sta_id];
                string time_str = reader.ReadString();
                TimeSpan time = TimeSpan.Parse(time_str);
                res.Arrivals.Add(sta, time);
            }

            int dep_count = reader.ReadInt32();
            for (int i = 0; i < dep_count; i++)
            {
                int sta_id = reader.ReadInt32();
                Station sta = stations[sta_id];
                string time_str = reader.ReadString();
                TimeSpan time = TimeSpan.Parse(time_str);
                res.Departures.Add(sta, time);
            }
            return res;
        }

        public Station DeserializeStation(BinaryReader reader)
        {
            var res = new Station();
            var name = reader.ReadString();
            var km = reader.ReadSingle();
            res.Attributes = DeserializeAttributes(reader);
            res.Name = name;
            res.Kilometre = km;
            return res;
        }

        public Dictionary<string, string> DeserializeAttributes(BinaryReader reader)
        {
            var res = new Dictionary<string, string>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                res.Add(key, value);
            }
            return res.Where(kvp => AttrMap.ContainsKey(kvp.Key)).ToDictionary(kvp => AttrMap[kvp.Key], kvp => kvp.Value);
        }

        private Dictionary<string, string> AttrMap = new Dictionary<string, string>()
        {
            ["MaxVelocity"] = "vmax",
            //TODO: jTG hinzufügen
        };
    }    
}
