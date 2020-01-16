namespace FPLedit.Shared
{
    public interface IEditMenuItemProxy
    {
        string DisplayName { get; }

        void Show(IPluginInterface pluginInterface);

        bool IsEnabled(IPluginInterface pluginInterface);
    }
}
