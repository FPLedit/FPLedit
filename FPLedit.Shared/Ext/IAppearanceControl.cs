using Eto.Forms;

namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable Proxy interface to provide a control that will be shown in the timetable appearance form (`Bearbeiten > Fahrplandarstellung`)
    /// </summary>
    public interface IAppearanceControl : IRegistrableComponent
    {
        /// <summary>
        /// Name that is used as display name in the type selector.
        /// </summary>
        /// <remarks>Must always return the same value.</remarks>
        string DisplayName { get; }

        /// <summary>
        /// Returns the Eto Control that will be used in the timetable appearance form.
        /// </summary>
        /// <remarks>The returned Control must implement <see cref="IAppearanceHandler"/> to be able to listen to events!</remarks>
        Control GetControl(IPluginInterface pluginInterface);
    }
}
