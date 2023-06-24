using FPLedit.Shared;
using FPLedit.GTFS.Model;
using Eto.Forms;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;

namespace FPLedit.GTFS.Forms
{
    internal sealed class SettingsControl : Panel, IAppearanceHandler
    {
        private readonly ISettings settings;
        private readonly GTFSAttrs attrs;

#pragma warning disable CS0649,CA2213
#pragma warning restore CS0649,CA2213

        public SettingsControl(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            var tt = pluginInterface.Timetable;
            settings = pluginInterface.Settings;

            attrs = GTFSAttrs.GetAttrs(tt) ?? GTFSAttrs.CreateAttrs(tt);
        }

        public void Save()
        {
        }

        public void SetExpertMode(bool enabled)
        {
        }

        public static class L
        {
            public static readonly string Example = T._("Beispiel");
            public static readonly string Template = T._("Aushangfahrplan-Vorlage");
            public static readonly string Css = T._("Eigene CSS-Styles");
            public static readonly string CssHelp = T._("Hilfe zu CSS");
            public static readonly string CssHelpLink = T._("https://fahrplan.manuelhu.de/dev/css/");
            public static readonly string ShowTracks = T._("Gleisangaben anzeigen");
            public static readonly string OmitSingleTracks = T._("Zeige Gleisangaben nicht, falls Bahnhof nur ein Gleis hat und kein Gleis am Zug definiert ist");
            public static readonly string Console = T._("CSS-Test-Konsole bei Vorschau aktivieren (Gilt für alle Fahrpläne)");
        }
    }
}
