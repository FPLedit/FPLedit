using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering
{
    public class ColorTimetableConverter
    {
        public static void ConvertAll(Timetable tt)
        {
            var timetableAttributes = new[] { "bgC", "hlC" };
            var stationAttributes = new[] { "cl" };
            var trainAttributes = new[] { "cl" };

            ConvertElement(tt.XMLEntity, timetableAttributes, tt.Version);

            foreach (var sta in tt.Stations)
                ConvertElement(sta.XMLEntity, stationAttributes, tt.Version);

            foreach (var tra in tt.Trains)
                ConvertElement(tra.XMLEntity, trainAttributes, tt.Version);
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
