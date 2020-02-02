using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [DebuggerDisplay("From {First} to {Next}")]
    [XElmName("tra")]
    [Templating.TemplateSafe]
    public class Transition : Entity
    {
        public Transition(Timetable tt) : base("tra", tt)
        {
        }

        public Transition(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [XAttrName("first")]
        public int First
        {
            get => GetAttribute<int>("first");
            set => SetAttribute("first", value.ToString());
        }

        [XAttrName("next")]
        public int Next
        {
            get => GetAttribute<int>("next");
            set => SetAttribute("next", value.ToString());
        }
    }
}
