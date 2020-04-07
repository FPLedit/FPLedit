namespace FPLedit.Shared
{
    /// <summary>
    /// Regsitrably proxy class to add custom menu items to the edit menu.
    /// </summary>
    public interface IEditMenuItemAction : IRegistrableComponent
    {
        /// <summary>
        /// Display name of the menu item.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// This method will be triggered when the menu item is triggered.
        /// </summary>
        void Invoke(IPluginInterface pluginInterface);

        /// <summary>
        /// The return value of this function will determine if the menu item is currently enabled.
        /// </summary>
        /// <remarks>Extensions may not block in this context.</remarks>
        bool IsEnabled(IPluginInterface pluginInterface);
    }
}
