using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public class PositionCollection
    {
        private Station sta;
        private Dictionary<int, float> positions;
        private TimetableType type;

        public PositionCollection(Station s, Timetable tt)
        {
            sta = s;
            positions = new Dictionary<int, float>();
            type = tt.Type;
            if (type == TimetableType.Linear)
                ParseLinear();
            else
                ParseNetwork();
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
            var toParse = sta.GetAttribute("km", "");
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
            var toParse = sta.GetAttribute("km", "0.0");
            positions.Add(0, float.Parse(toParse, CultureInfo.InvariantCulture));
        }

        private void Write()
        {
            if (type == TimetableType.Linear)
            {
                sta.SetAttribute("km", GetPosition(0).Value.ToString("0.0", CultureInfo.InvariantCulture));
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
