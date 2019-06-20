using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    [XElmName("track")]
    public class Track : Entity
    {
        public Track(Timetable tt) : base("track", tt)
        {
        }

        public Track(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [XAttrName("name")]
        public string Name
        {
            get => GetAttribute<string>("name");
            set => SetAttribute("name", value);
        }
    }
}
