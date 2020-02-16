namespace FPLedit.Shared
{
    public interface IEditMenuItemProxy
    {
        string DisplayName { get; }

        void Invoke(IPluginInterface pluginInterface);

        bool IsEnabled(IPluginInterface pluginInterface);
    }
}
