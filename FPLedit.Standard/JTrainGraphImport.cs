using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FPLedit.Standard
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
                        Attributes = staAtts,
                    });
                }

                string line1 = stas.First().Name + " - " + stas.Last().Name;
                string line2 = stas.Last().Name + " - " + stas.First().Name;

                XElement trains = el.Element("trains");
                foreach (var train in trains.Elements())
                {
                    string name = train.Attribute("name").Value;
                    Dictionary<Station, ArrDep> ardps = new Dictionary<Station, ArrDep>();

                    int i = 0;
                    foreach (var time in train.Elements())
                    {
                        ArrDep ardp = new ArrDep();
                        if (time.Attribute("a").Value != "")
                            ardp.Arrival = TimeSpan.Parse(time.Attribute("a").Value);

                        if (time.Attribute("d").Value != "")
                            ardp.Departure = TimeSpan.Parse(time.Attribute("d").Value);
                        ardps[stas.ElementAt(i)] = ardp;
                        i++;
                    }

                    var trAtts = train.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

                    var dir = train.Name.LocalName == "ti" ? TrainDirection.ti : TrainDirection.ta;
                    trs.Add(new Train()
                    {
                        Attributes = trAtts,
                        ArrDeps = ardps,
                        Direction = dir,
                        Line = dir.Get() ? line2 : line1
                    });
                }

                var ttAtts = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

                return new Timetable()
                {
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
    }
}
