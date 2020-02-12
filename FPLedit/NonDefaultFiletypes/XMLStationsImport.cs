using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class XmlStationsImport : IImport
    {
        public string Filter => "Streckendateien (*.str)|*.str";

        public Timetable Import(Stream stream, IPluginInterface pluginInterface, ILog replaceLog = null)
        {
            var xElement = XElement.Load(stream);

            var xmlEntity = new XMLEntity(xElement);
            var list = new StationsList(xmlEntity);
            
            var tt = new Timetable(TimetableType.Linear);
            foreach (var i in list.Stations)
                tt.AddStation(i, Timetable.LINEAR_ROUTE_ID);
            
            return tt;
        }
    }
}
