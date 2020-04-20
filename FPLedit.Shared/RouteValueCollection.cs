using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    /// <summary>
    /// A RouteValueCollection (RVC) allows to define properties which allow for different values for each individual
    /// route of a Station.
    /// </summary>
    /// <typeparam name="T">Any type that is transformable to a string with reasonable effort.</typeparam>
    [Templating.TemplateSafe]
    public class RouteValueCollection<T>
    {
        private static readonly EscapeSplitHelper escape = new EscapeSplitHelper(';');
        
        private readonly IEntity entity;
        private readonly Dictionary<int, T> values;
        private readonly Timetable tt;
        private readonly string attr, defaultVal;
        private readonly Func<string, T> convTo;
        private readonly Func<T, string> convFrom;
        private readonly bool optional;
        private readonly T convDefault;

        /// <summary>
        /// Creates a new RVC.
        /// </summary>
        /// <param name="e">The entity this RVC should operate on, read values from &amp; write vTalues to.</param>
        /// <param name="tt">The parent timetable of the entity <paramref name="e"/>.</param>
        /// <param name="attr">The XML attribute name this RVC should read from and write to.</param>
        /// <param name="defaultVal">Default value that is used when the given attribute is not present. As serialized string.</param>
        /// <param name="convTo">Function to serialize <typeparamref name="T"/> to a string. Special rules apply. See remarks.</param>
        /// <param name="convFrom">Function to deserialize <typeparamref name="T"/> from a string. Special rules apply. See remarks.</param>
        /// <param name="optional">Specifies if this attribute is optional.</param>
        /// <remarks>
        /// The character ";" will be escaped in the serialized string.
        /// </remarks>
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

        /// <summary>
        /// This method does nothing, but can be used to test for errors if the RVC is constructed on-demand.
        /// </summary>
        public void TestForErrors()
        {
        }

        /// <summary>
        /// Return the value - or the default value, if no value has been set - corresponding to the given route.
        /// </summary>
        public T GetValue(int route)
        {
            if (values.TryGetValue(route, out T val))
                return val;
            return convDefault;
        }

        /// <summary>
        /// Set the value corresponding to the given route.
        /// </summary>
        public void SetValue(int route, T val)
        {
            values[route] = val;
            Write();
        }

        /// <summary>
        /// Parse the attribute data from a multi-route context (e.g. Network timetable).
        /// </summary>
        private void ParseNetwork()
        {
            var toParse = entity.GetAttribute(attr, "");
            var rts = escape.SplitEscaped(toParse);
            foreach (var p in rts)
            {
                var parts = p.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                var route = int.Parse(parts[0]);
                var value = convTo(parts.Length == 2 ? parts[1] : defaultVal);
                values.Add(route, value);
            }
        }

        /// <summary>
        /// Parse the attribute if there is only one route and we are not in a multi-route context (e.g. linear timetable).
        /// </summary>
        private void ParseLinear()
        {
            var toParse = entity.GetAttribute<string>(attr, null);
            if (optional && toParse == null)
                return;
            
            if (toParse == null)
                toParse = defaultVal;
            values.Add(Timetable.LINEAR_ROUTE_ID, convTo(toParse));
        }

        /// <summary>
        /// Write all values back to the XML attribute.
        /// </summary>
        /// <param name="forceType">Force either network or linear mode (only to be used by conversions!).</param>
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
                var posStrings = values.Select(kvp => kvp.Key + ":" + convFrom(kvp.Value));
                text = escape.JoinEscaped(posStrings);
            }
            entity.SetAttribute(attr, text);
        }

        /// <summary>
        /// Returns whether this RVC contains the given value at any route.
        /// </summary>
        public bool ContainsValue(T value) => values.ContainsValue(value);

        /// <summary>
        /// Replace all opccurences of <paramref name="oldVal"/> with <paramref name="newVal" /> on all routes. 
        /// </summary>
        public void ReplaceAllValues(T oldVal, T newVal)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var kvp = values.ElementAt(i);
                if (kvp.Value.Equals(oldVal))
                    SetValue(kvp.Key, newVal);
            }
        }
    }
}

