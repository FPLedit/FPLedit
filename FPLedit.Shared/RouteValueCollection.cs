using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public class RouteValueCollection<T>
    {
        private readonly IEntity entity;
        private Dictionary<int, T> values;
        private readonly Timetable tt;
        private string attr, defaultVal;
        private Func<string, T> convTo;
        private Func<T, string> convFrom;
        private bool optional;
        private T convDefault;

        public RouteValueCollection(IEntity e, Timetable tt, string attr, string defaultVal, Func<string, T> convTo, Func<T, string> convFrom, bool optional = true)
        {
            this.attr = attr;
            this.convFrom = convFrom;
            this.convTo = convTo;
            this.defaultVal = defaultVal;
            this.optional = optional;
            convDefault = convTo(defaultVal);

            entity = e;
            values = new Dictionary<int, T>();
            this.tt = tt;
            if (tt.Type == TimetableType.Linear)
                ParseLinear();
            else
                ParseNetwork();
        }

        public void TestForErrors() // Do nothing. Contructor is enough.
        {
        }

        public T GetValue(int route)
        {
            if (values.TryGetValue(route, out T val))
                return val;
            return convDefault;
        }

        public void SetValue(int route, T val)
        {
            values[route] = val;
            Write();
        }

        private void ParseNetwork()
        {
            var toParse = entity.GetAttribute(attr, "");
            var rts = toParse.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in rts)
            {
                var parts = p.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                values.Add(int.Parse(parts[0]), convTo(parts[1]));
            }
        }

        private void ParseLinear()
        {
            var toParse = entity.GetAttribute<string>(attr, null);
            if (optional && toParse == null)
                return;
            else if (toParse == null)
                toParse = defaultVal;
            values.Add(Timetable.LINEAR_ROUTE_ID, convTo(toParse));
        }

        public void Write(TimetableType? forceType = null)
        {
            // Skip, if collection is empty & attribute is optional
            if (optional && !values.Any())
                return;

            var text = "";
            var t = forceType ?? tt.Type;
            if (t == TimetableType.Linear)
                text = convFrom(GetValue(Timetable.LINEAR_ROUTE_ID));
            else
            {
                var posStrings = values.Select(kvp => kvp.Key.ToString() + ":" + convFrom(kvp.Value));
                text = string.Join(";", posStrings);
            }
            entity.SetAttribute(attr, text);
        }
    }
}

