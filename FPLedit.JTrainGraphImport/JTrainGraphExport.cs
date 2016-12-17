using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FPLedit.JTrainGraphImport
{
    public class JTrainGraphExport : IExport
    {
        public string Filter
        {
            get
            {
                return "jTrainGraph Fahrplan Dateien (*.fpl)|*.xml";
            }
        }

        public bool Reoppenable
        {
            get
            {
                return true;
            }
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            XElement ttElm = new XElement("jTrainGraph_timetable");
            foreach (var attr in tt.Attributes)
                ttElm.SetAttributeValue(attr.Key, attr.Value);

            XElement stasElm = new XElement("stations");
            ttElm.Add(stasElm);
            foreach (var sta in tt.Stations)
            {
                XElement staElm = new XElement("sta");
                foreach (var attr in sta.Attributes)
                    staElm.SetAttributeValue(attr.Key, attr.Value);
                stasElm.Add(staElm);
            }

            XElement trasElement = new XElement("trains");
            ttElm.Add(trasElement);
            foreach (var tra in tt.Trains)
            {
                XElement traElm = new XElement(tra.Direction.ToString());
                foreach (var attr in tra.Attributes)
                    traElm.SetAttributeValue(attr.Key, attr.Value);
                foreach (var ardep in tra.ArrDeps)
                {
                    var tElm = new XElement("t");
                    var ar = ardep.Value.Arrival.ToShortTimeString();
                    var dp = ardep.Value.Departure.ToShortTimeString();
                    tElm.SetAttributeValue("a", ar != "00:00" ? ar : "");
                    tElm.SetAttributeValue("d", dp != "00:00" ? dp : "");
                    traElm.Add(tElm);
                }
                trasElement.Add(traElm);
            }

            using (var writer = new XmlTextWriter(filename, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                ttElm.Save(writer);
            }
            return true;
        }
    }
}
