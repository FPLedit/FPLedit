using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [Templating.TemplateSafe]
    public class PositionCollection
    {
        private readonly IStation sta;
        private readonly Dictionary<int, float> positions;
        private readonly Timetable tt;

        public PositionCollection(IStation s, Timetable tt)
        {
            sta = s;
            positions = new Dictionary<int, float>();
            this.tt = tt;
            if (tt.Type == TimetableType.Linear)
                ParseLinear();
            else
                ParseNetwork();
        }

        public void TestForErrors() // Do nothing. Contructor is enough.
        {
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
            string toParse;
            if (tt.Version == TimetableVersion.JTG2_x)
                toParse = sta.GetAttribute("km", "0.0");
            else // jTG 3.0
            {
                toParse = sta.GetAttribute("kml", "0.0");
                var kmr = sta.GetAttribute("kmr", "0.0");
                if (toParse != kmr)
                    throw new NotSupportedException("Unterschiedliche kmr/kml werden aktuell von FPLedit nicht unterstützt!");
            }
            positions.Add(Timetable.LINEAR_ROUTE_ID, float.Parse(toParse, CultureInfo.InvariantCulture));
        }

        public void Write(TimetableType? forceType = null, TimetableVersion? forceVersion = null)
        {
            var t = forceType ?? tt.Type;
            var v = forceVersion ?? tt.Version;
            if (t == TimetableType.Linear)
            {
                var pos = GetPosition(Timetable.LINEAR_ROUTE_ID).Value.ToString("0.0", CultureInfo.InvariantCulture);
                if (v == TimetableVersion.JTG2_x)
                {
                    sta.SetAttribute("km", pos);
                    sta.RemoveAttribute("kml");
                    sta.RemoveAttribute("kmr");
                }
                else // jTG 3.0
                {
                    sta.SetAttribute("kml", pos);
                    sta.SetAttribute("kmr", pos);
                    sta.RemoveAttribute("km");
                }
            }
            else
            {
                var posStrings = positions.Select(kvp => kvp.Key.ToString() + ":" + kvp.Value.ToString("0.0", CultureInfo.InvariantCulture));
                var text = string.Join(";", posStrings);
                sta.SetAttribute("km", text);
                sta.RemoveAttribute("kml");
                sta.RemoveAttribute("kmr");
            }
        }
    }
}
