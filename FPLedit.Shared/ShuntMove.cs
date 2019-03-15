using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    public class ShuntMove : Entity
    {
        public ShuntMove(Timetable tt) : base("shMove", tt)
        {
        }

        public ShuntMove(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public string SourceTrack
        {
            get => GetAttribute<string>("so");
            set => SetAttribute("so", value);
        }

        public string TargetTrack
        {
            get => GetAttribute<string>("ta");
            set => SetAttribute("ta", value);
        }

        public TimeSpan Time
        {
            get => GetTime("ti");
            set => SetNotEmptyTime(value, "ti");
        }

        public bool EmptyAfterwards
        {
            get => GetAttribute<bool>("ea");
            set => SetAttribute("ea", value.ToString().ToLower());
        }
    }
}
