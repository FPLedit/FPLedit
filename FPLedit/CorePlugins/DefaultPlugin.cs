#nullable enable
using FPLedit.Shared;

namespace FPLedit.CorePlugins
{
    internal sealed class DefaultPlugin : IPlugin
    {
        private readonly Bootstrapper bootstrapper;
        private readonly IRestartable restartable;

        public DefaultPlugin(IRestartable restartable, Bootstrapper bootstrapper)
        {
            this.restartable = restartable;
            this.bootstrapper = bootstrapper;
        }

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            componentRegistry.Register<IExport>(new NonDefaultFiletypes.CleanedXmlExport());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.TransitionsCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.TrainsTrackCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.DayOverflowCheck());
            componentRegistry.Register<ITimetableCheck>(new TimetableChecks.StationCodeCheck());
            componentRegistry.Register<ITimetableInitAction>(new TimetableChecks.BugFixInitAction());
            componentRegistry.Register<ITimetableInitAction>(new TimetableChecks.FixNetworkAttributesAction());
            
            componentRegistry.Register<IImport>(new NonDefaultFiletypes.XmlStationsImport());
            componentRegistry.Register<IExport>(new NonDefaultFiletypes.StationsOnlyExport());
            
            componentRegistry.Register<ISettingsControl>(new SettingsUi.ExtensionsControlHandler(bootstrapper.ExtensionManager, restartable));
            componentRegistry.Register<ISettingsControl>(new SettingsUi.TemplatesControlHandler());
            componentRegistry.Register<ISettingsControl>(new SettingsUi.AutomaticUpdateControl());
            componentRegistry.Register<ISettingsControl>(new SettingsUi.UiSettingsControl());
            componentRegistry.Register<ISettingsControl>(new SettingsUi.LocaleControl());
            componentRegistry.Register<ISettingsControl>(new SettingsUi.DefaultVersionControl());
        }
    }
}