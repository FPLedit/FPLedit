namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable action hook that is executed once a timetable file is loaded.
    /// </summary>
    public interface ITimetableInitAction : IRegistrableComponent
    {
        string Init(Timetable tt);
    }
}
