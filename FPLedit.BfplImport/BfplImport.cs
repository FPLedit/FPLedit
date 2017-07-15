using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.BfplImport
{
    public class BfplImport : IImport
    {
        public const string MAGIC = "BFPL/1.1"; // letzte "Magic number" des BFPL-Formats

        public string Filter => "Alte FPLedit Dateien (*.bfpl)|*.bfpl";

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

        private Timetable DeserializeTimetable(BinaryReader reader)
        {
            Timetable res = new Timetable();

            string magic = reader.ReadString();
            if (magic != MAGIC)
                throw new Exception("Falsche Dateiversion!");

            var name = reader.ReadString();
            res.Attributes = DeserializeAttributes(reader, EntityType.Timetable);
            res.TTName = name;

            var stations = new Dictionary<int, Station>();
            int sta_count = reader.ReadInt32();
            for (int i = 0; i < sta_count; i++)
            {
                int id = reader.ReadInt32();
                var sta = DeserializeStation(reader, res);
                stations.Add(id, sta);
                res.AddStation(sta);
            }

            int tra_count = reader.ReadInt32();
            for (int i = 0; i < tra_count; i++)
            {
                var tr = DeserializeTrain(reader, stations, res);
                res.AddTrain(tr, true);
            }

            res.Attributes["version"] = "008";

            return res;
        }

        private Train DeserializeTrain(BinaryReader reader, Dictionary<int, Station> stations, Timetable tt)
        {
            var name = reader.ReadString();
            var tfz = reader.ReadString();
            var dir = reader.ReadBoolean() ? TrainDirection.ta : TrainDirection.ti;
            reader.ReadString(); // Line nicht mehr im Model
            Train res = new Train(dir, tt);
            var days = DaysHelper.ParseDays(reader.ReadString());
            res.Attributes = DeserializeAttributes(reader, EntityType.Train);
            res.TName = name;
            res.Days = days;
            res.Locomotive = tfz;

            var ars = new Dictionary<Station, TimeSpan>();
            var dps = new Dictionary<Station, TimeSpan>();

            int arr_count = reader.ReadInt32();
            for (int i = 0; i < arr_count; i++)
            {
                int sta_id = reader.ReadInt32();
                Station sta = stations[sta_id];
                string time_str = reader.ReadString();
                TimeSpan time = TimeSpan.Parse(time_str);
                ars.Add(sta, time);
            }

            int dep_count = reader.ReadInt32();
            for (int i = 0; i < dep_count; i++)
            {
                int sta_id = reader.ReadInt32();
                Station sta = stations[sta_id];
                string time_str = reader.ReadString();
                TimeSpan time = TimeSpan.Parse(time_str);
                dps.Add(sta, time);
            }

            foreach (var station in stations.Values)
            {
                var ardp = new ArrDep();
                if (ars.ContainsKey(station))
                    ardp.Arrival = ars[station];
                if (dps.ContainsKey(station))
                    ardp.Departure = dps[station];
                res.AddArrDep(station, ardp);
            }

            return res;
        }

        private Station DeserializeStation(BinaryReader reader, Timetable tt)
        {
            var res = new Station(tt);
            var name = reader.ReadString();
            var km = reader.ReadSingle();
            res.Attributes = DeserializeAttributes(reader, EntityType.Station);
            res.SName = name;
            res.Kilometre = km;

            return res;
        }

        private Dictionary<string, string> DeserializeAttributes(BinaryReader reader, EntityType type)
        {
            var res = new Dictionary<string, string>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                res.Add(key, value);
            }
            return UpgradeMeta.Upgrade(type, res);
        }
    }

    enum EntityType
    {
        Train,
        Timetable,
        Station
    }
}
