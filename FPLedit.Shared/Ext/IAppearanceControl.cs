using Eto.Forms;

namespace FPLedit.Shared
{
    public interface IAppearanceControl
    {
        string DisplayName { get; }

        Control GetControl(IPluginInterface pluginInterface);
    }
}
