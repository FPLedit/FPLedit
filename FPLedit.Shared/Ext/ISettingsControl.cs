using Eto.Forms;

namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable Proxy interface to provide a control that will be shown in the settings form.
    /// </summary>
    public interface ISettingsControl : IRegistrableComponent
    {
        /// <summary>
        /// Name that is used as display name in the type selector.
        /// </summary>
        /// <remarks>Must always return the same value.</remarks>
        string DisplayName { get; }

        /// <summary>
        /// Returns the Eto Control that will be used in the settings form.
        /// </summary>
        Control GetControl(IPluginInterface pluginInterface);
    }
}
