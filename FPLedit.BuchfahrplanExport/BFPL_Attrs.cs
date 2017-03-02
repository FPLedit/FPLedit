using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    public class BFPL_Attrs : Entity
    {
        public List<BFPL_Point> Points { get; private set; }

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


        public BFPL_Attrs(Timetable tt) : base("bfpl_attrs", tt)
        {

        }

        public BFPL_Attrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
            Points = new List<BFPL_Point>();
            foreach (var c in en.Children.Where(x => x.XName == "p")) // Filtert andere Elemente
                Points.Add(new BFPL_Point(c, _parent));
        }

        public void AddPoint(BFPL_Point p)
        {
            Points.Add(p);
            //TODO: Maybe sort by position?
            XMLEntity.Children.Add(p.XMLEntity);
        }

        public void RemovePoint(BFPL_Point p)
        {
            Points.Remove(p);
            XMLEntity.Children.Remove(p.XMLEntity);
        }
    }
}
