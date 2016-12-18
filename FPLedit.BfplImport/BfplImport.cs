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
            
            foreach (var attr in timetableDefaultAttrs)
                if (!res.Attributes.ContainsKey(attr.Key))
                    res.Attributes[attr.Key] = attr.Value;
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
                res.ArrDeps[station] = ardp;
            }
            
            foreach (var attr in trainDefaultAttrs)
                if (!res.Attributes.ContainsKey(attr.Key))
                    res.Attributes[attr.Key] = attr.Value;
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
            
            foreach (var attr in stationDefaultAttrs)
                if (!res.Attributes.ContainsKey(attr.Key))
                    res.Attributes[attr.Key] = attr.Value;
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
            return res.Where(kvp => upgradeAttrMap.ContainsKey(kvp.Key)).ToDictionary(kvp => upgradeAttrMap[kvp.Key], kvp => kvp.Value);
        }

        private Dictionary<string, string> upgradeAttrMap = new Dictionary<string, string>()
        {
            ["MaxVelocity"] = "vmax",
            //TODO: jTG hinzufügen
        };

        Dictionary<string, string> timetableDefaultAttrs = new Dictionary<string, string>()
        {
            ["version"] = "008",
            ["name"] = "",
            ["tMin"] = "330",
            ["tMax"] = "1410",
            ["d"] = "1111111",
            ["bgC"] = "weiß",
            ["sFont"] = "font(SansSerif;0;12)",
            ["trFont"] = "font(SansSerif;0;12)",
            ["hFont"] = "font(SansSerif;0;12)",
            ["tFont"] = "font(SansSerif;0;12)",
            ["sHor"] = "false",
            ["sLine"] = "0",
            ["shKm"] = "true",
            ["sStation"] = "-1",
            ["eStation"] = "-1",
            ["cNr"] = "1",
            ["exW"] = "-1",
            ["exH"] = "-1",
            ["shV"] = "true", // StationLines
        };

        Dictionary<string, string> trainDefaultAttrs = new Dictionary<string, string>()
        {
            ["name"] = "",
            ["cm"] = "",
            ["cl"] = "schwarz",
            ["sh"] = "true",
            ["sz"] = "1",
            ["sy"] = "0",
            ["d"] = "1111111",
            ["id"] = "",
        };

        Dictionary<string, string> stationDefaultAttrs = new Dictionary<string, string>()
        {
            ["name"] = "",
            ["km"] = "0.0",
            ["cl"] = "schwarz",
            ["sh"] = "true",
            ["sz"] = "1",
            ["sy"] = "0",
        };
    }
}
