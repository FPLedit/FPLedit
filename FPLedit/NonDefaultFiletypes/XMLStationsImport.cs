using FPLedit.Shared;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class XmlStationsImport : IImport
    {
        public string Filter => "Lineare Streckendateien (*.str)|*.str";

        public Timetable Import(Stream stream, IReducedPluginInterface pluginInterface, ILog replaceLog = null)
        {
            var xElement = XElement.Load(stream);

            var xmlEntity = new XMLEntity(xElement);
            var tt = new Timetable(TimetableType.Linear);
            
            var stations = xmlEntity.Children.Where(x => x.XName == "sta") // Filters other elements
                .Select(x => new Station(x, tt));
            
            foreach (var i in stations)
                tt.AddStation(i, Timetable.LINEAR_ROUTE_ID);
            
            return tt;
        }
    }
}
