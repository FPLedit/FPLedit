namespace FPLedit.Shared
{
    public interface IEditMenuItemAction
    {
        string DisplayName { get; }

        void Invoke(IPluginInterface pluginInterface);

        bool IsEnabled(IPluginInterface pluginInterface);
    }
}
