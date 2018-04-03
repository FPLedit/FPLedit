using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    //TODO: Handle kmr/kml in a better, more generic way
    [Serializable]
    public class PositionCollection
    {
        private IStation sta;
        private Dictionary<int, float> positions;
        private TimetableType type;
        private TimetableVersion version;

        public PositionCollection(IStation s, Timetable tt)
        {
            sta = s;
            positions = new Dictionary<int, float>();
            type = tt.Type;
            version = tt.Version;
            if (type == TimetableType.Linear)
                ParseLinear();
            else
                ParseNetwork();
        }

        public void TestForErrors()
        {
            // Do nothing. Contructor is enough.
        }

        public float? GetPosition(int route)
        {
            if (positions.TryGetValue(route, out float val))
                return val;
            return null;
        }

        public void SetPosition(int route, float km)
        {
            positions[route] = km;
            Write();
        }

        private void ParseNetwork()
        {
            var toParse = sta.GetAttribute("km", ""); // Format EXTENDED_FPL, km ist gut
            var pos = toParse.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pos)
            {
                var parts = p.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                positions.Add(int.Parse(parts[0]),
                    float.Parse(parts[1], CultureInfo.InvariantCulture));
            }
        }

        private void ParseLinear()
        {
            var toParse = "";
            if (version == TimetableVersion.JTG2_x)
                toParse = sta.GetAttribute("km", "0.0");
            else // jTG 3.0
                toParse = sta.GetAttribute("kml", "0.0");
            positions.Add(Timetable.LINEAR_ROUTE_ID, float.Parse(toParse, CultureInfo.InvariantCulture));
        }

        private void Write()
        {
            if (type == TimetableType.Linear)
            {
                var pos = GetPosition(Timetable.LINEAR_ROUTE_ID).Value.ToString("0.0", CultureInfo.InvariantCulture);
                if (version == TimetableVersion.JTG2_x)
                    sta.SetAttribute("km", pos);
                else // jTG 3.0
                {
                    sta.SetAttribute("kml", pos);
                    sta.SetAttribute("kmr", pos);
                }
            }
            else
            {
                var posStrings = positions.Select(kvp => kvp.Key.ToString() + ":" + kvp.Value.ToString("0.0", CultureInfo.InvariantCulture));
                var text = string.Join(";", posStrings);
                sta.SetAttribute("km", text);
            }
        }
    }
}
