namespace FPLedit.Shared
{
    /// <summary>
    /// Helper interface for plugins that provide builtin templates. This interface has no effect on external plugins and should only be used by FPLedit internally!
    /// </summary>
    public interface ITemplatePlugin
    {
        /// <summary>
        /// This function is not called by FPLedit (only the FPLedit unit test environment) when loading plugins that contain templates.
        /// It is the resposibility of the implementor to call this method in a normal bootstrap process (e.g. in <see cref="IPlugin.Init"/>).
        /// </summary>
        void InitTemplates(IPluginInterface pluginInterface, IComponentRegistry componentRegistry);
    }
}
