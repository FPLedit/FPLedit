using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Standard
{
    /*public class BfplExport : IExport
    {
        public string Filter
        {
            get
            {
                return "FPLedit Datei (*.bfpl)|*.bfpl";
            }
        }

        public bool Reoppenable
        {
            get { return true; }
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            try
            {
                using (FileStream stream = File.Open(filename, FileMode.OpenOrCreate))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        SerializeTimetable(writer, tt);
                    return true;
                }                
            }
            catch (Exception ex)
            {
                logger.Error("BfplExport: " + ex.Message);
                return false;
            }
        }

        public void SerializeTimetable(BinaryWriter writer, Timetable tt)
        {
            writer.Write(Timetable.MAGIC);
            tt.Name = "";
            writer.Write(tt.Name);
            SerializeMeta(writer, tt);

            var stations = new Dictionary<Station, int>();
            writer.Write(tt.Stations.Count);
            int i = 0;
            foreach (var sta in tt.Stations)
            {
                writer.Write(i); // STA_UNIQ_ID

                writer.Write(sta.Name);
                writer.Write(sta.Kilometre);
                SerializeMeta(writer, sta);

                stations.Add(sta, i);
                i++;
            }

            writer.Write(tt.Trains.Count);
            foreach (var tra in tt.Trains)
            {
                writer.Write(tra.Name);
                writer.Write(tra.Locomotive);
                writer.Write(tra.Direction);
                writer.Write(tra.Line);
                writer.Write(tra.DaysToBinString());
                SerializeMeta(writer, tra);

                writer.Write(tra.Arrivals.Count);
                foreach (var item in tra.Arrivals)
                {
                    writer.Write(stations[item.Key]);
                    writer.Write(item.Value.ToString());
                }

                writer.Write(tra.Departures.Count);
                foreach (var item in tra.Departures)
                {
                    writer.Write(stations[item.Key]);
                    writer.Write(item.Value.ToString());
                }
            }
        }

        public void SerializeMeta(BinaryWriter writer, Entity m)
        {
#pragma warning disable CS0612
            writer.Write(m.Metadata.Count);
            foreach (var pair in m.Metadata)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
#pragma warning restore CS0612
        }
    }*/
}
