using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class XMLStationsImport : IImport
    {
        public string Filter => "Streckendateien (*.str)|*.str";

        public Timetable Import(Stream stream, IPluginInterface pluginInterface, ILog replaceLog = null)
        {
            try
            {
                XElement el = XElement.Load(stream);

                XMLEntity en = new XMLEntity(el);
                var list = new StationsList(en);
                var tt = new Timetable(TimetableType.Linear);
                foreach (var i in list.Stations)
                    tt.AddStation(i, Timetable.LINEAR_ROUTE_ID);
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
