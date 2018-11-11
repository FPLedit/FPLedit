using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("From {First} to {Next}")]
    public class Transition : Entity
    {
        public Transition(Timetable tt) : base("tra", tt)
        {
        }

        public Transition(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public int First
        {
            get => GetAttribute<int>("first");
            set => SetAttribute("first", value.ToString());
        }

        public int Next
        {
            get => GetAttribute<int>("next");
            set => SetAttribute("next", value.ToString());
        }
    }
}
