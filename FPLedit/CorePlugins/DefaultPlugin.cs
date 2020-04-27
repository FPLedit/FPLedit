using FPLedit.Shared;

namespace FPLedit.CorePlugins
{
    public sealed class DefaultPlugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            componentRegistry.Register<IExport>(new NonDefaultFiletypes.CleanedXmlExport());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.TransitionsCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.TrainsTrackCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.DayOverflowCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.StationCodeCheck());
            componentRegistry.Register<ITimetableInitAction>(new TimetableChecks.BugFixInitAction());
            componentRegistry.Register<ITimetableInitAction>(new TimetableChecks.UpdateColorsAction());
            componentRegistry.Register<ITimetableInitAction>(new TimetableChecks.FixNetworkAttributesAction());
            
            componentRegistry.Register<IImport>(new NonDefaultFiletypes.XmlStationsImport());
        }
    }
}