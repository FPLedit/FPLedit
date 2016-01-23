using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Buchfahrplan
{
    public static class FplImport
    {
        public static Timetable Import(string filename)
        {
            List<Station> stas = new List<Station>();
            List<Train> trs = new List<Train>();
            XElement el = XElement.Load(filename);

            XElement stations = el.Element("stations");
            foreach (var station in stations.Elements())
            {
                stas.Add(new Station()
                {
                    Name = station.Attribute("name").Value,
                    Kilometre = float.Parse(station.Attribute("km").Value, CultureInfo.InvariantCulture)
                });
            }

            string line1 = stas.First().Name + " - " + stas.Last().Name;
            string line2 = stas.Last().Name + " - " + stas.First().Name;

            XElement trains = el.Element("trains");
            foreach (var train in trains.Elements())
            {
                Dictionary<Station, DateTime> ar = new Dictionary<Station, DateTime>();
                Dictionary<Station, DateTime> dp = new Dictionary<Station, DateTime>();
                int i = 0;

                foreach (var time in train.Elements())
                {
                    try { ar.Add(stas.ElementAt(i), DateTime.ParseExact(time.Attribute("a").Value, "HH:mm", CultureInfo.InvariantCulture)); }
                    catch { }

                    try { dp.Add(stas.ElementAt(i), DateTime.Parse(time.Attribute("d").Value)); }
                    catch { }
                    i++;
                }

                bool neg = IsNegative(ar, dp, stas.First(), stas.Last());
                trs.Add(new Train()
                {
                    Name = train.Attribute("name").Value,
                    Arrivals = ar,
                    Departures = dp,
                    Negative = neg,
                    Line = neg ? line2 : line1
                });
            }

            return new Timetable()
            {
                Name = el.Attribute("name").Value,
                Stations = stas,
                Trains = trs
            };
        }

        private static bool IsNegative(Dictionary<Station, DateTime> ar, Dictionary<Station, DateTime> dp, Station first, Station last)
        {
            DateTime firsttime = ar.ContainsKey(first) ? ar.First().Value : dp.First().Value;
            DateTime lasttime = ar.ContainsKey(last) ? ar.Last().Value : dp.Last().Value;

            return firsttime > lasttime;
        }
    }
}
