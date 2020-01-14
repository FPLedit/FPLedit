using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal class XMLStationsImport : IImport
    {
        public string Filter => "Streckendateien (*.str)|*.str";

        public Timetable Import(string filename, IPluginInterface pluginInterface, ILog replaceLog = null)
        {
            try
            {
                XElement el = XElement.Load(filename);

                XMLEntity en = new XMLEntity(el);
                var list = new StationsList(en);
                var tt = new Timetable(TimetableType.Linear);
                foreach (var i in list.Stations)
                    tt.AddStation(i, 0);
                return tt;
            }
            catch (Exception ex)
            {
                var log = replaceLog ?? pluginInterface.Logger;
                log.Error("XMLStationsImporter: " + ex.Message);
                return null;
            }
        }
    }
}
