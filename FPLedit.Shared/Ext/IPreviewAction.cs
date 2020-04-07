namespace FPLedit.Shared
{
    /// <summary>
    /// Registrable component to provide a menu item in the preview menu.
    /// </summary>
    /// <remarks><see cref="DefaultImplementations.DefaultPreview"/> for a default implementation.</remarks>
    public interface IPreviewAction : IRegistrableComponent
    {
        /// <summary>
        /// Display name of the preview.
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Text of the menu item.
        /// </summary>
        string MenuName { get; }

        /// <summary>
        /// This method will be invoked when the action is triggered.
        /// </summary>
        void Show(IPluginInterface pluginInterface);
    }
}
