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
        private float position;
        private TimetableVersion version;

        public PositionCollection(IStation s, Timetable tt)
        {
            sta = s;
            version = tt.Version;
            ParseLinear();
        }

        public void TestForErrors()
        {
            // Do nothing. Contructor is enough.
        }

        public float? GetPosition()
        {
            return position;
        }

        public void SetPosition(float km)
        {
            position = km;
            Write();
        }

        private void ParseLinear()
        {
            var toParse = "";
            if (version == TimetableVersion.JTG2_x)
                toParse = sta.GetAttribute("km", "0.0");
            else // jTG 3.0
                toParse = sta.GetAttribute("kml", "0.0");
            position = float.Parse(toParse, CultureInfo.InvariantCulture);
        }

        private void Write()
        {
            var pos = position.ToString("0.0", CultureInfo.InvariantCulture);
            if (version == TimetableVersion.JTG2_x)
                sta.SetAttribute("km", pos);
            else // jTG 3.0
            {
                sta.SetAttribute("kml", pos);
                sta.SetAttribute("kmr", pos);
            }
        }
    }
}
