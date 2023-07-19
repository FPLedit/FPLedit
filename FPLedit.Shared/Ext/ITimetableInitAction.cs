namespace FPLedit.Shared;

/// <summary>
/// Registrable action hook that is executed once a timetable file is loaded.
/// </summary>
public interface ITimetableInitAction : IRegistrableComponent
{
    /// <summary>
    /// Hook that is executed after the timetable instance has just been constructed from th XML tree.
    /// </summary>
    /// <param name="tt">The writable copy of the current Timetable.</param>
    /// <param name="pluginInterface"></param>
    /// <returns>Any warning message that is shown to the user, otherwise null.</returns>
    string? Init(Timetable tt, IReducedPluginInterface pluginInterface);
}