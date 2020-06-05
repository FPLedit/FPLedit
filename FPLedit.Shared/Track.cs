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
        
        /// <summary>
        /// Create a copy of this Track. This is not a deep copy, as it copies only the xml tree below, and it preserves the parent timetable.
        /// </summary>
        public Track Copy()
        {
            var xml = XMLEntity.XClone();
            return new Track(xml, ParentTimetable);
        }
    }
}
