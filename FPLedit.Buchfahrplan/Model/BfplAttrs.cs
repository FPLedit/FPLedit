using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan.Model
{
    [XElmName("bfpl_attrs", IsFpleditElement = true)]
    public sealed class BfplAttrs : Entity, IPatternSource
    {
        public List<BfplPoint> Points { get; }

        [XAttrName("font")]
        public string Font
        {
            get => GetAttribute("font", "");
            set => SetAttribute("font", value);
        }

        [XAttrName("tmpl")]
        public string Template
        {
            get => GetAttribute("tmpl", "");
            set => SetAttribute("tmpl", value);
        }

        [XAttrName("css")]
        public string Css
        {
            get
            {
                var val = Children.FirstOrDefault(x => x.XName == "css")?.Value ?? "";
                var bytes = Convert.FromBase64String(val);
                return Encoding.UTF8.GetString(bytes);
            }
            set
            {
                var bytes = Encoding.UTF8.GetBytes(value);

                var elm = Children.FirstOrDefault(x => x.XName == "css");
                if (elm == null)
                {
                    elm = new XMLEntity("css");
                    Children.Add(elm);
                }
                elm.Value = Convert.ToBase64String(bytes);
            }
        }

        [XAttrName("tp")]
        public string TrainPatterns
        {
            get => GetAttribute("tp", "");
            set => SetAttribute("tp", value);
        }

        [XAttrName("sp")]
        public string StationPatterns
        {
            get => GetAttribute("sp", "");
            set => SetAttribute("sp", value);
        }

        [XAttrName("shC")]
        public bool ShowComments
        {
            get => GetAttribute<bool>("shC");
            set => SetAttribute("shC", value.ToString().ToLower());
        }

        [XAttrName("shD")]
        public bool ShowDays
        {
            get => GetAttribute<bool>("shD");
            set => SetAttribute("shD", value.ToString().ToLower());
        }

        private BfplAttrs(Timetable tt) : base("bfpl_attrs", tt)
        {
            Points = new List<BfplPoint>();
        }

        private BfplAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
            Points = new List<BfplPoint>();
            foreach (var c in en.Children.Where(x => x.XName == "p")) // Filtert andere Elemente
                Points.Add(new BfplPoint(c, _parent));
        }

        public static BfplAttrs GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");
            if (attrsEn != null)
                return new BfplAttrs(attrsEn, tt);
            return null;
        }

        public BfplPoint[] GetRoutePoints(int route)
        {
            if (_parent.Type == TimetableType.Linear && route == Timetable.LINEAR_ROUTE_ID)
                return Points.ToArray();
            return Points.Where(p => p.Routes.Contains(route)).ToArray();
        }

        public void AddPoint(BfplPoint p)
        {
            Points.Add(p);
            XMLEntity.Children.Add(p.XMLEntity);
        }

        public void RemovePoint(BfplPoint p)
        {
            Points.Remove(p);
            XMLEntity.Children.Remove(p.XMLEntity);
        }
        
        public static BfplAttrs CreateAttrs(Timetable tt)
        {
            var attrs = new BfplAttrs(tt);
            tt.Children.Add(attrs.XMLEntity);
            return attrs;
        }
    }
}
