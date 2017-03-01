using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes
{
    public class XMLStationsImport : IImport
    {
        public string Filter
        {
            get
            {
                return "Streckendateien (*.str)|*.str";
            }
        }

        public Timetable Import(string filename, ILog logger)
        {
            try
            {
                XElement el = XElement.Load(filename);

                XMLEntity en = new XMLEntity(el);
                var list = new StationsList(en);
                var tt = new Timetable();
                foreach (var i in list.Stations)
                    tt.AddStation(i);
                return tt;
            }
            catch (Exception ex)
            {
                logger.Error("XMLStationsImporter: " + ex.Message);
                return null;
            }
        }
    }
}
