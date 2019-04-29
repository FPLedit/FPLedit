using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering
{
    public class ColorTimetableConverter
    {
        public static void ConvertAll(Timetable tt, TimetableVersion? forceVersion = null)
        {
            var timetableAttributes = new[] { "bgC", "hlC" };
            var stationAttributes = new[] { "cl" };
            var trainAttributes = new[] { "cl" };

            var version = forceVersion ?? tt.Version;

            ConvertElement(tt.XMLEntity, timetableAttributes, version);

            foreach (var sta in tt.Stations)
                ConvertElement(sta.XMLEntity, stationAttributes, version);

            foreach (var tra in tt.Trains)
                ConvertElement(tra.XMLEntity, trainAttributes, version);
        }

        private static void ConvertElement(XMLEntity xml, string[] attributesToConvert, TimetableVersion version)
        {
            foreach (var xe in xml.Attributes.ToArray())
            {
                if (attributesToConvert.Contains(xe.Key))
                    xml.SetAttribute(xe.Key, ConvertColor(xe.Value, version));
            }
        }

        private static string ConvertColor(string value, TimetableVersion version)
        {
            var mcolor = ColorFormatter.FromString(value, null);
            return ColorFormatter.ToString(mcolor, version == TimetableVersion.JTG2_x);
        }
    }
}
