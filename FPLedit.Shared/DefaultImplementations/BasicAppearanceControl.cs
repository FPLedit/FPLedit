using System;
using Eto.Forms;
using FPLedit.Shared;

namespace FPLedit.Shared.DefaultImplementations
{
    public class BasicAppearanceControl : IAppearanceControl
    {
        private readonly Func<IPluginInterface, Control> getControl;
        public string DisplayName { get; }

        public Control GetControl(IPluginInterface pluginInterface)
            => getControl(pluginInterface);

        public BasicAppearanceControl(Func<IPluginInterface, Control> getControl, string displayName)
        {
            this.getControl = getControl;
            DisplayName = displayName;
        }
    }
}
