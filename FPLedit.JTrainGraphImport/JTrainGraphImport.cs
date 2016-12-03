using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FPLedit.JTrainGraphImport
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
                    Dictionary<Station, TimeSpan> ar = new Dictionary<Station, TimeSpan>();
                    Dictionary<Station, TimeSpan> dp = new Dictionary<Station, TimeSpan>();
                    Dictionary<string, string> md = new Dictionary<string, string>();

                    bool[] days = Train.ParseDays(train.Attribute("d").Value);

                    string color = colors.ContainsKey(train.Attribute("cl").Value) ? colors[train.Attribute("cl").Value] : null;
                    if (color != null)
                        md.Add("Color", color);

                    int i = 0;
                    foreach (var time in train.Elements())
                    {
                        if (time.Attribute("a").Value != "")
                            ar.Add(stas.ElementAt(i), TimeSpan.Parse(time.Attribute("a").Value));

                        if (time.Attribute("d").Value != "")
                            dp.Add(stas.ElementAt(i), TimeSpan.Parse(time.Attribute("d").Value));
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
                        Line = dir ? line2 : line1,
                        Metadata = md
                    });
                }

                return new Timetable()
                {
                    Name = el.Attribute("name").Value,
                    Stations = stas,
                    Trains = trs
                };
            }
            catch (Exception ex)
            {
                logger.Error("JTrainGraphImporter: " + ex.Message);
                return null;
            }
        }

        private bool GetDirection(Dictionary<Station, TimeSpan> ar, Dictionary<Station, TimeSpan> dp, Station first, Station last)
        {
            TimeSpan firsttime = ar.ContainsKey(first) ? ar.First().Value : dp.First().Value;
            TimeSpan lasttime = ar.ContainsKey(last) ? ar.Last().Value : dp.Last().Value;

            return firsttime > lasttime;
        }        

        private Dictionary<string, string> colors = new Dictionary<string, string>()
        {
            ["schwarz"] = "#000000",
            ["grau"] = "#808080",
            ["weiß"] = "#FFFFFF",
            ["rot"] = "#FF0000",
            ["orange"] = "#FFA500",
            ["gelb"] = "#FFFF00",
            ["blau"] = "#0000FF",
            ["hellblau"] = "#ADD8E6",
            ["grün"] = "#008000",
            ["dunkelgrün"] = "#006400",
            ["braun"] = "#A52A2A",
            ["magenta"] = "#FF00FF"
        };
    }
}
