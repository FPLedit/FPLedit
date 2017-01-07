using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    public class BFPL_Attrs : Entity
    {
        public string Font
        {
            get
            {
                return GetAttribute("font", "");
            }
            set
            {
                SetAttribute("font", value);
            }
        }

        public string Css
        {
            get
            {
                return Children.FirstOrDefault(x => x.XName == "css")?.Value;
            }
            set
            {
                var elm = Children.FirstOrDefault(x => x.XName == "css");
                if (elm == null)
                {
                    elm = new XMLEntity("css");
                    Children.Add(elm);
                }
                elm.Value = value;
            }
        }


        public BFPL_Attrs(Timetable tt) : base("bfpl_attrs", tt)
        {

        }

        public BFPL_Attrs(XMLEntity en, Timetable tt) : base(en, tt)
        {

        }
    }
}
