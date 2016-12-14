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

            res.Name = reader.ReadString();
            res.Metadata = DeserializeMeta(reader);

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
            res.Name = reader.ReadString();
            res.Locomotive = reader.ReadString();
            res.Direction = reader.ReadBoolean();
            res.Line = reader.ReadString();
            res.Days = Train.ParseDays(reader.ReadString());
            res.Metadata = DeserializeMeta(reader);

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
            res.Name = reader.ReadString();
            res.Kilometre = reader.ReadSingle();
            res.Metadata = DeserializeMeta(reader);
            return res;
        }

        public Dictionary<string, string> DeserializeMeta(BinaryReader reader)
        {
            var res = new Dictionary<string, string>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                res.Add(key, value);
            }
            return res;
        }
    }
}
