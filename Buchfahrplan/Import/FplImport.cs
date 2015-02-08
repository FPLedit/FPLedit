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

             XElement trains = el.Element("trains");
             foreach (var train in trains.Elements())
             {
                 Dictionary<Station, DateTime> ar = new Dictionary<Station, DateTime>();
                 Dictionary<Station, DateTime> dp = new Dictionary<Station, DateTime>();
                 int i = 0;

                 foreach (var time in train.Elements())
                 {
                     //MessageBox.Show(time.Attribute("a").Value);
                     try { ar.Add(stas.ElementAt(i), DateTime.ParseExact(time.Attribute("a").Value, "HH:mm", CultureInfo.InvariantCulture)); }
                     catch { }

                     try { dp.Add(stas.ElementAt(i), DateTime.Parse(time.Attribute("d").Value)); }
                     catch { }
                     i++;
                 }

                 trs.Add(new Train()
                 {
                     Name = train.Attribute("name").Value, 
                     Arrivals = ar, 
                     Departures = dp                     
                 });
             }

             return new Timetable()
             {
                 Name = el.Attribute("name").Value,
                 Stations = stas,
                 Trains = trs
             };
        }
    }
}
