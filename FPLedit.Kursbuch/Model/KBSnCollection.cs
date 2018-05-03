using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch.Model
{
    public class KBSnCollection
    {
        private KfplAttrs attrs;
        private Dictionary<int, string> kbsns;
        private TimetableType type;

        private const string ATTR = "kbsn";

        public KBSnCollection(KfplAttrs attrs, Timetable tt)
        {
            this.attrs = attrs;
            kbsns = new Dictionary<int, string>();
            type = tt.Type;
            if (type == TimetableType.Linear)
                ParseLinear();
            else
                ParseNetwork();
        }

        public string GetKbsn(int route)
        {
            if (kbsns.TryGetValue(route, out string val))
                return val;
            return null;
        }

        public void SetKbsn(int route, string kbsn)
        {
            kbsns[route] = kbsn;
            Write();
        }

        private void ParseNetwork()
        {
            var toParse = attrs.GetAttribute(ATTR, "");
            var pos = toParse.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in pos)
            {
                var parts = p.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                kbsns.Add(int.Parse(parts[0]), parts[1]);
            }
        }

        private void ParseLinear()
        {
            var toParse = attrs.GetAttribute<string>(ATTR);
            kbsns.Add(Timetable.LINEAR_ROUTE_ID, toParse);
        }

        private void Write()
        {
            if (type == TimetableType.Linear)
                attrs.SetAttribute(ATTR, GetKbsn(Timetable.LINEAR_ROUTE_ID));
            else
            {
                var posStrings = kbsns.Select(kvp => kvp.Key.ToString() + ":" + kvp.Value);
                var text = string.Join(";", posStrings);
                attrs.SetAttribute(ATTR, text);
            }
        }
    }
}
