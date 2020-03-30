using FPLedit.Shared;

namespace FPLedit.CorePlugins
{
    public sealed class DefaultPlugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            pluginInterface.Register<IExport>(new NonDefaultFiletypes.CleanedXmlExport());
            pluginInterface.Register<ITimetableCheck>(new TimetableChecks.TransitionsCheck());
            pluginInterface.Register<ITimetableCheck>(new TimetableChecks.TrainsTrackCheck());
            pluginInterface.Register<ITimetableCheck>(new TimetableChecks.DayOverflowCheck());
            pluginInterface.Register<ITimetableInitAction>(new TimetableChecks.UpdateColorsAction());
            pluginInterface.Register<ITimetableInitAction>(new TimetableChecks.FixNetworkAttributesAction());
        }
    }
}