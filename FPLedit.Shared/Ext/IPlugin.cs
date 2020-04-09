namespace FPLedit.Shared
{
    /// <summary>
    /// Base interface for all extensions.
    /// </summary>
    /// <remarks>All external inheritors must have a <see cref="PluginAttribute"/> to be discovered by FPLedit.</remarks>
    public interface IPlugin
    {
        /// <summary>
        /// This function is when the actived plugin is initialized at application start-up.
        /// </summary>
        /// <param name="pluginInterface">The Plugin Interface is the main object to hook into FPLedit program flow.</param>
        /// <param name="componentRegistry">The registry can be used to register components that extend specific areas of FPLedit.</param>
        /// <remarks>
        /// Not all properties of the Plugin interface are already ready to be used. See <see cref="IPluginInterface"/>,
        /// <see cref="IReducedPluginInterface"/> and <see cref="IUiPluginInterface"/> for details.
        /// </remarks>
        void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry);
    }
}
