using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;

namespace FPLedit.SettingsUi
{
    internal sealed class DefaultVersionControl : ISettingsControl
    {
        public string DisplayName => T._("Dateiversionen");

        public Control GetControl(IPluginInterface pluginInterface)
        {
            var linVersions = GetAvailableVersions(TimetableType.Linear);
            var netVersions = GetAvailableVersions(TimetableType.Network);

#pragma warning disable CA2000
            var linLabel = new Label { Text = T._("Lineare Fahrpläne") };
            var netLabel = new Label { Text = T._("Netzwerk-Fahrpläne") };

            var descriptionLabel = new Label
            {
                Text = T._("Hier kann die Dateiformatversion ausgewählt werden, mit der standardmäßig neue Dateien erstellt werden." +
                           "\n\nDie Auwahl der höchsten Dateiversion ist meist die richtige Lösung. Nur in Ausnahmefällen ist eine Anpassung " +
                           "sinnvoll, beispielsweise wenn eine ältere Version von jTrainGraph verwendet werden soll.\n" +
                           "Ein Öffnen von neueren Dateien in älteren Programmversionen ist oft nicht möglich.")
            };

            var linearDropDown = new DropDown { DataStore = linVersions.Cast<object>(), SelectedValue = Timetable.DefaultLinearVersion };
            linearDropDown.SelectedValueChanged += (s, e) =>
            {
                var linVersion = (TimetableVersion) linearDropDown.SelectedValue;
                Timetable.DefaultLinearVersion = linVersion;
                pluginInterface.Settings.SetEnum("core.default-file-format", linVersion);
            };

            var networkDropDown = new DropDown { DataStore = netVersions.Cast<object>(), SelectedValue = Timetable.DefaultNetworkVersion };
            networkDropDown.SelectedValueChanged += (s, e) =>
            {
                var netVersion = (TimetableVersion) networkDropDown.SelectedValue;
                Timetable.DefaultNetworkVersion = netVersion;
                pluginInterface.Settings.SetEnum("core.default-network-file-format", netVersion);
            };
#pragma warning restore CA2000

            var table = new TableLayout(new TableRow(new TableCell(), descriptionLabel),
                    new TableRow(linLabel, linearDropDown),
                    new TableRow(netLabel, networkDropDown),
                    new TableRow() { ScaleHeight = true })
                { Spacing = new Size(5, 5) };

            return table;
        }

        private TimetableVersion[] GetAvailableVersions(TimetableType type)
        {
            return TimetableVersionExt.GetAllVersionInfos()
                .Where(c => c.Compatibility == TtVersionCompatType.ReadWrite && c.Type == type)
                .Select(c => c.Version)
                .ToArray();
        }
    }
}