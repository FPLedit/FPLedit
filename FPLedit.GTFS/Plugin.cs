using FPLedit.GTFS.Forms;
using FPLedit.GTFS.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.GTFS
{
    [Plugin("Modul für GTFS-Export", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            //componentRegistry.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), T._("GTFS")));
            componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);
        }

        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer(T._("GTFS"), GTFSAttrs.GetAttrs, GTFSAttrs.CreateAttrs);
    }
}
