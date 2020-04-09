namespace FPLedit.Shared
{
    /// <summary>
    /// This object model type represents a single point in the network, bound on one or more "Routes".
    /// </summary>
    [Templating.TemplateSafe]
    public interface IStation : IEntity
    {
        /// <summary>
        /// Unique identifier of this station. Might throw on not-supported timetable types (e.g. linear) and on
        /// unsupported implementors that are not <see cref="Station"/>s.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Name of the station.
        /// </summary>
        string SName { get; set; }

        /// <summary>
        /// Positions on the line on all routes this station serves. ("chainage")
        /// </summary>
        PositionCollection Positions { get; }

        /// <summary>
        /// Optional metadata for track gradient from this point on. Depends on route index. May be shown in some outputs.
        /// On German "Buchfahrplan" this is show as one two three waves on the side of the table, hence the name.
        /// </summary>
        RouteValueCollection<int> Wellenlinien { get;}

        /// <summary>
        /// Optional metadata for maximum velocity from this point on. Depends on route index. May be shown in some outputs.
        /// </summary>
        RouteValueCollection<string> Vmax { get; }

        /// <summary>
        /// Returns all routes this station serves. This is available on all timetable types.
        /// </summary>
        int[] Routes { get; }
    }
}
