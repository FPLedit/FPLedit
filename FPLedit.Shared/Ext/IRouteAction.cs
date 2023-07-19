namespace FPLedit.Shared;

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
    /// Icon of type <see cref="Eto.Drawing.Bitmap" />, will be used instead of the display text if not null, and configured by the user.
    /// </summary>
    /// <remarks>The icon size should be at least 64x64 px.</remarks>
    dynamic? EtoIconBitmap { get; }

    /// <summary>
    /// This method will be triggered when the action is invoked.
    /// </summary>
    /// <param name="pluginInterface">The current plugin interface instance.</param>
    /// <param name="route">The currently selected route, or <see langword="null" /> if no route is selected (or no file is opened).</param>
    void Invoke(IPluginInterface pluginInterface, Route? route);

    /// <summary>
    /// The return value of this function will determine if the button is currently enabled.
    /// </summary>
    /// <remarks>Extensions may not block in this context.</remarks>
    bool IsEnabled(IPluginInterface pluginInterface);
}