using FPLedit.Shared;
using System;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch.Model
{
    [XElmName("kfpl_attrs", IsFpleditElement = true)]
    public sealed class KfplAttrs : Entity, IPatternSource
    {
        [XAttrName("font")]
        public string Font
        {
            get => GetAttribute("font", "");
            set => SetAttribute("font", value);
        }

        [XAttrName("hefont")]
        public string HeFont
        {
            get => GetAttribute("hefont", "");
            set => SetAttribute("hefont", value);
        }

        [XAttrName("kbsn")]
        public RouteValueCollection<string> KBSn
            => new RouteValueCollection<string>(this, ParentTimetable, "kbsn", "", s => s, s => s);

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

        [XAttrName("tmpl")]
        public string Template
        {
            get => GetAttribute("tmpl", "");
            set => SetAttribute("tmpl", value);
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

        private KfplAttrs(Timetable tt) : base("kfpl_attrs", tt)
        {
        }

        private KfplAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public static KfplAttrs? GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "kfpl_attrs");
            if (attrsEn != null)
                return new KfplAttrs(attrsEn, tt);
            return null;
        }
        
        public static KfplAttrs CreateAttrs(Timetable tt)
        {
            var attrs = new KfplAttrs(tt);
            tt.Children.Add(attrs.XMLEntity);
            return attrs;
        }
    }
}
