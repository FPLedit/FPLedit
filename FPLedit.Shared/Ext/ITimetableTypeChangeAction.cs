namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable action hook that is executed once a timetable file is converted between Linear and Network Timetable
    /// types, which allows to add functionality to those conversion steps.
    /// </summary>
    public interface ITimetableTypeChangeAction : IRegistrableComponent
    {
        /// <summary>
        /// This hook is executed when the timetable is converted to a linear timetable, after removing network attributes
        /// (Station ID, Route information) of all the stations, but before performing any other conversion step.
        /// </summary>
        /// <remarks>The API is not in a sane state and mioght throw unexpected errors. Use the XML-API directly, when possible.</remarks>
        /// <param name="tt">The writable timetable instance.</param>
        void ToLinear(Timetable tt);

        /// <summary>
        /// This hook is executed when the timetable is converted to a network timetable, after adding network attributes
        /// (Station ID, Route information) to all the stations, but before performing any other conversion step.
        /// </summary>
        /// <remarks>The API is not in a sane state and mioght throw unexpected errors. Use the XML-API directly, when possible.</remarks>
        /// <param name="tt">The writable timetable instance.</param>
        void ToNetwork(Timetable tt);
    }
}
