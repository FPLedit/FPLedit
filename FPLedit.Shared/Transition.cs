using System.Diagnostics;

namespace FPLedit.Shared
{
    /// <summary>
    /// Object-model type to contain a single trainsition between to trains ("first" and "next"). This may result in different output.
    /// </summary>
    /// <remarks>A transition means that one train is run with the same locomotive, staff, cars or similar, but this is not enforced by FPLedit.</remarks>
    [DebuggerDisplay("From {First} to {Next}")]
    [XElmName("tra")]
    [Templating.TemplateSafe]
    public sealed class Transition : Entity
    {
        public Transition(Timetable tt) : base("tra", tt)
        {
        }

        public Transition(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [XAttrName("first")]
        public string First
        {
            get => GetAttribute<string>("first");
            set => SetAttribute("first", value.ToString());
        }

        [XAttrName("next")]
        public string Next
        {
            get => GetAttribute<string>("next");
            set => SetAttribute("next", value.ToString());
        }
    }
}
