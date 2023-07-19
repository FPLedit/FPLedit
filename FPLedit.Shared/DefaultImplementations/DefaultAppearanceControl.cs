using System;
using Eto.Forms;

namespace FPLedit.Shared.DefaultImplementations;

public class DefaultAppearanceControl : IAppearanceControl
{
    private readonly Func<IPluginInterface, Control> getControl;
    public string DisplayName { get; }

    public Control GetControl(IPluginInterface pluginInterface)
        => getControl(pluginInterface);

    public DefaultAppearanceControl(Func<IPluginInterface, Control> getControl, string displayName)
    {
        this.getControl = getControl;
        DisplayName = displayName;
    }
}