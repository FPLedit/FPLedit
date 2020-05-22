namespace FPLedit.Shared
{
    /// <summary>
    /// Represents a single track definition of a <see cref="Station"/>, as used in <see cref="Station.Tracks"/>
    /// </summary>
    [XElmName("track")]
    [Templating.TemplateSafe]
    public sealed class Track : Entity
    {
        public Track(Timetable tt) : base("track", tt)
        {
        }

        /// <inheritdoc />
        public Track(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [XAttrName("name")]
        public string Name
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }
    }
}
