using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Aushangfahrplan.Model
{
    public class AfplAttrs : Entity
    {
        public string Font
        {
            get => GetAttribute("font", "");
            set => SetAttribute("font", value);
        }

        public string HwFont
        {
            get => GetAttribute("hwfont", "");
            set => SetAttribute("hwfont", value);
        }

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

        public bool ShowTracks
        {
            get => GetAttribute<bool>("shT", true);
            set => SetAttribute("shT", value.ToString().ToLower());
        }

        public AfplAttrs(Timetable tt) : base("afpl_attrs", tt)
        {
        }

        public AfplAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public static AfplAttrs GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "afpl_attrs");
            if (attrsEn != null)
                return new AfplAttrs(attrsEn, tt);
            return null;
        }
    }
}
