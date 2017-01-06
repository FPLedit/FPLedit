using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    public class BFPL_Data : Entity
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


        public BFPL_Data(string xn, Timetable tt) : base(xn, tt)
        {

        }

        public BFPL_Data(XMLEntity en, Timetable tt) : base(en, tt)
        {

        }
    }
}
