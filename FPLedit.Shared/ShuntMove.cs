namespace FPLedit.Shared
{
    [XElmName("shMove")]
    [Templating.TemplateSafe]
    public sealed class ShuntMove : Entity
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
        public TimeEntry Time
        {
            get => GetTimeAttributeValue("ti");
            set => SetNotEmptyTimeAttribute("ti", value);
        }

        [XAttrName("ea")]
        public bool EmptyAfterwards
        {
            get => GetAttribute<bool>("ea");
            set => SetAttribute("ea", value.ToString().ToLower());
        }
    }
}
