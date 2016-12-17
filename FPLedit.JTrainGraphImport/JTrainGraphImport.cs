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
                    var staAtts = station.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);
                    stas.Add(new Station()
                    {
                        /*Name = station.Attribute("name").Value,
                        Kilometre = float.Parse(station.Attribute("km").Value, CultureInfo.InvariantCulture)*/
                        Attributes = staAtts,
                    });
                }

                string line1 = stas.First().Name + " - " + stas.Last().Name;
                string line2 = stas.Last().Name + " - " + stas.First().Name;

                XElement trains = el.Element("trains");
                foreach (var train in trains.Elements())
                {
                    string name = train.Attribute("name").Value;
                    //Dictionary<Station, TimeSpan> ar = new Dictionary<Station, TimeSpan>();
                    //Dictionary<Station, TimeSpan> dp = new Dictionary<Station, TimeSpan>();
                    //Dictionary<string, string> md = new Dictionary<string, string>();
                    Dictionary<Station, ArrDep> ardps = new Dictionary<Station, ArrDep>();

                    //bool[] days = Train.ParseDays(train.Attribute("d").Value);

                    int i = 0;
                    foreach (var time in train.Elements())
                    {
                        ArrDep ardp = new ArrDep();
                        if (time.Attribute("a").Value != "")
                            ardp.Arrival = TimeSpan.Parse(time.Attribute("a").Value);
                            //ar.Add(stas.ElementAt(i), TimeSpan.Parse(time.Attribute("a").Value));

                        if (time.Attribute("d").Value != "")
                            ardp.Departure = TimeSpan.Parse(time.Attribute("d").Value);
                        ardps[stas.ElementAt(i)] = ardp;
                        i++;
                    }

                    var trAtts = train.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

                    //bool dir = GetDirection(ar, dp);

                    var dir = train.Name.LocalName == "ti" ? TrainDirection.ti : TrainDirection.ta;
                    trs.Add(new Train()
                    {
                        //Name = name,
                        Attributes = trAtts,
                        //Arrivals = ar,
                        //Departures = dp,
                        ArrDeps = ardps,
                        Direction = dir,
                        //Days = days,
                        Line = dir.Get() ? line2 : line1
                    });
                }

                var ttAtts = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

                return new Timetable()
                {
                    //Name = el.Attribute("name").Value,
                    Attributes = ttAtts,
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

        /*private bool GetDirection(Dictionary<Station, TimeSpan> ar, Dictionary<Station, TimeSpan> dp)
        {
            float? lastArrival = ar.OrderBy(a => a.Value).LastOrDefault().Key?.Kilometre;
            float? firstDeparture = dp.OrderBy(d => d.Value).FirstOrDefault().Key?.Kilometre;
            return firstDeparture > lastArrival;
        }*/
    }
}
