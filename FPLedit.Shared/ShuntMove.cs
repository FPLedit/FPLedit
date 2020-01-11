using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    [XElmName("shMove")]
    [TemplateSafe]
    public class ShuntMove : Entity
    {
        public ShuntMove(Timetable tt) : base("shMove", tt)
        {
        }

        public ShuntMove(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [XAttrName("so")]
        public string SourceTrack
        {
            get => GetAttribute<string>("so");
            set => SetAttribute("so", value);
        }

        [XAttrName("ta")]
        public string TargetTrack
        {
            get => GetAttribute<string>("ta");
            set => SetAttribute("ta", value);
        }

        [XAttrName("ti")]
        public TimeSpan Time
        {
            get => GetTimeValue("ti");
            set => SetNotEmptyTime(value, "ti");
        }

        [XAttrName("ea")]
        public bool EmptyAfterwards
        {
            get => GetAttribute<bool>("ea");
            set => SetAttribute("ea", value.ToString().ToLower());
        }
    }
}
