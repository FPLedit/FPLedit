using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Buchfahrplan.JTrainGraphImport
{
    public class JTrainGraphImport : IImport
    {
        public string Filter
        {
            get
            {
                return "jTrainGraph Fahrplan Dateien (*.fpl)|*.fpl";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
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
                    string name = train.Attribute("name").Value;
                    Dictionary<Station, DateTime> ar = new Dictionary<Station, DateTime>();
                    Dictionary<Station, DateTime> dp = new Dictionary<Station, DateTime>();
                    int i = 0;

                    bool[] days = ParseDays(train.Attribute("d").Value);

                    foreach (var time in train.Elements())
                    {
                        if (time.Attribute("a").Value != "")
                            ar.Add(stas.ElementAt(i), DateTime.ParseExact(time.Attribute("a").Value, "HH:mm", CultureInfo.InvariantCulture));

                        if (time.Attribute("d").Value != "")
                            dp.Add(stas.ElementAt(i), DateTime.Parse(time.Attribute("d").Value));
                        i++;
                    }

                    bool dir = GetDirection(ar, dp, stas.First(), stas.Last());
                    trs.Add(new Train()
                    {
                        Name = name,
                        Arrivals = ar,
                        Departures = dp,
                        Direction = dir,
                        Days = days,
                        Line = dir ? line2 : line1
                    });
                }

                NewEditForm newEdit = new NewEditForm();
                newEdit.Init(trs);
                if (newEdit.ShowDialog() == DialogResult.OK)
                    trs = newEdit.trains;

                return new Timetable()
                {
                    Name = el.Attribute("name").Value,
                    Stations = stas,
                    Trains = trs
                };
            }
            catch (Exception ex)
            {
                logger.Log("JTrainGraphImporter: Error: " + ex.Message);
                return null;
            }
        }

        private bool GetDirection(Dictionary<Station, DateTime> ar, Dictionary<Station, DateTime> dp, Station first, Station last)
        {
            DateTime firsttime = ar.ContainsKey(first) ? ar.First().Value : dp.First().Value;
            DateTime lasttime = ar.ContainsKey(last) ? ar.Last().Value : dp.Last().Value;

            return firsttime > lasttime;
        }

        private bool[] ParseDays(string att)
        {
            bool[] days = new bool[7];
            char[] chars = att.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                days[i] = chars[i] == '1';
            return days;
        }
    }
}
