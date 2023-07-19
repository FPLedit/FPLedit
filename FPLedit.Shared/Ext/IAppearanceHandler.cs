namespace FPLedit.Shared;

/// <summary>
/// Controls returned by <see cref="IAppearanceControl"/> must implement this. It is not registrable.
/// </summary>
public interface IAppearanceHandler
{
    /// <summary>
    /// This method is called, when the user triggers a save action in the timetable appearance form.
    /// </summary>
    void Save();

    /// <summary>
    /// This method is called, when the user enables or disables expert mode.
    /// </summary>
    void SetExpertMode(bool enabled);
}