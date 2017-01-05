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
            //try
            //{


                //List<Station> stas = new List<Station>();
                //List<Train> trs = new List<Train>();
                XElement el = XElement.Load(filename);

            //XElement trains = el.Element("trains");
            //foreach (var train in trains.Elements())
            //{
            //    Dictionary<Station, ArrDep> ardps = new Dictionary<Station, ArrDep>();

            //    int i = 0;
            //    foreach (var time in train.Elements())
            //    {
            //        ArrDep ardp = new ArrDep();
            //        if (time.Attribute("a").Value != "")
            //            ardp.Arrival = TimeSpan.Parse(time.Attribute("a").Value);

            //        if (time.Attribute("d").Value != "")
            //            ardp.Departure = TimeSpan.Parse(time.Attribute("d").Value);
            //        ardps[stas.ElementAt(i)] = ardp;
            //        i++;
            //    }

            //    var trAtts = train.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

            //    var dir = train.Name.LocalName == "ti" ? TrainDirection.ti : TrainDirection.ta;
            //    trs.Add(new Train()
            //    {
            //        Attributes = trAtts,
            //        ArrDeps = ardps,
            //    });
            //}

            //var ttAtts = el.Attributes().ToDictionary(a => a.Name.LocalName, a => (string)a);

            XMLEntity en = new XMLEntity(el, null);
            return new Timetable(en);
                //{
                //    //Attributes = ttAtts,
                //    //Stations = stas,
                //    //Trains = trs
                //};



            //}
            //catch (Exception ex)
            //{
            //    logger.Error("JTrainGraphImporter: " + ex.Message);
            //    return null;
            //}
        }
    }
}
