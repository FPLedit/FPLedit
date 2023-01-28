namespace FPLedit.Shared
{
    /// <summary>
    /// Regsitrable action that exposes a route-dependant edit action as a button on the network editor.
    /// </summary>
    public interface IRouteAction : IRegistrableComponent
    {
        /// <summary>
        /// Display name of the button, as it is shown on the Network editor toolbar.
        /// </summary>
        /// <remarks>The character "&amp;" can be used as a keyboard shortcut, to enable use without pointing device.</remarks>
        string DisplayName { get; }

        /// <summary>
        /// This method will be triggered when the action is invoked.
        /// </summary>
        void Invoke(IPluginInterface pluginInterface, Route route);

        /// <summary>
        /// The return value of this function will determine if the button is currently enabled.
        /// </summary>
        /// <remarks>Extensions may not block in this context.</remarks>
        bool IsEnabled(IPluginInterface pluginInterface);
    }
}
