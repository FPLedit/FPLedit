using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch.Model
{
    public class KfplAttrs : Entity
    {
        public string Font
        {
            get => GetAttribute("font", "");
            set => SetAttribute("font", value);
        }

        public string HeFont
        {
            get => GetAttribute("hefont", "");
            set => SetAttribute("hefont", value);
        }

        public KBSnCollection KBSn
            => new KBSnCollection(this, _parent);

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

        public string Template
        {
            get => GetAttribute("tmpl", "");
            set => SetAttribute("tmpl", value);
        }

        public string TrainPatterns
        {
            get => GetAttribute("tp", "");
            set => SetAttribute("tp", value);
        }

        public string StationPatterns
        {
            get => GetAttribute("sp", "");
            set => SetAttribute("sp", value);
        }

        public KfplAttrs(Timetable tt) : base("kfpl_attrs", tt)
        {
        }

        public KfplAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public static KfplAttrs GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "kfpl_attrs");
            if (attrsEn != null)
                return new KfplAttrs(attrsEn, tt);
            return null;
        }
    }
}
