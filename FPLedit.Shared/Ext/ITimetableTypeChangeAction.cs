namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable action hook that is executed once a timetable file is converted between Linear and Network Timetable types.
    /// </summary>
    public interface ITimetableTypeChangeAction : IRegistrableComponent
    {
        void ToLinear(Timetable tt);

        void ToNetwork(Timetable tt);
    }
}
