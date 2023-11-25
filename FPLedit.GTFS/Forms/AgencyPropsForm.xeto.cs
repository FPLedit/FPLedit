using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using FPLedit.Shared.UI;
using FPLedit.GTFS.GTFSLib;
using FPLedit.GTFS.Model;

namespace FPLedit.GTFS.Forms;

internal sealed class AgencyPropsForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox agencyNameTextBox = null!, agencyLangTextBox = null!, agencyUrlTextBox = null!, agencyTimezoneTextBox = null!, routeNameTextBox = null!, daysOverrideTextBox = null!;
    private readonly DropDown routeTypeDropDown = null!;
#pragma warning restore CS0649,CA2213
    private readonly ValidatorCollection validators;

    private readonly GtfsAttrs attrs;
    private readonly Timetable tt;
    private readonly IPluginInterface pluginInterface;
    private readonly object backupHandle;

    public AgencyPropsForm(IPluginInterface pluginInterface)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        backupHandle = pluginInterface.BackupTimetable();

        this.tt = pluginInterface.Timetable;
        this.pluginInterface = pluginInterface;

        var daysOverrideValidator = new RegexValidator(daysOverrideTextBox, GtfsDays.FullDateRegex, true, errorMessage: T._("Bitte gültige Datumsspezifikation eintragen!"));
        validators = new ValidatorCollection(daysOverrideValidator);

        agencyNameTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.AgencyName);
        agencyLangTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.AgencyLang);
        agencyUrlTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.AgencyUrl);
        agencyTimezoneTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.AgencyTimezone);

        routeNameTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.RouteName);
        DropDownBind.Enum<GtfsAttrs, RouteType>(routeTypeDropDown, nameof(GtfsAttrs.RouteTypeEnum), null);

        daysOverrideTextBox.TextBinding.BindDataContext<GtfsAttrs>(s => s.DaysOverride);

        attrs = GtfsAttrs.GetAttrs(tt) ?? GtfsAttrs.CreateAttrs(tt);
        DataContext = attrs;

        this.AddCloseHandler();
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (!validators.IsValid)
        {
            MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", validators.Message));
            return;
        }

        pluginInterface.ClearBackup(backupHandle);

        Result = DialogResult.Ok;
        this.NClose();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        pluginInterface.RestoreTimetable(backupHandle);

        Result = DialogResult.Cancel;
        this.NClose();
    }

    private static class L
    {
        public static readonly string AgencyHeading = T._("Betreiberdaten:");
        public static readonly string AgencyName = T._("Name d. Betreibers");
        public static readonly string AgencyLang = T._("Sprachcode d. Betreibers (z.B. „de“ oder „de-DE“)");
        public static readonly string AgencyUrl = T._("Website d. Betreibers");
        public static readonly string AgencyTimezone = T._("Zeitzone d. Betreibers (z.B. „Europe/Berlin“)");

        public static readonly string RouteHeading = T._("Streckendaten:");
        public static readonly string RouteName = T._("Name der (einen) Strecke");
        public static readonly string RouteType = T._("Typ des Beförderungsmittels");

        public static readonly string TrainHeading = T._("Globale Zugdaten:");
        public static readonly string DaysOverride = T._("Verkehrstage der GTFS-Züge");

        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("GTFS-Agency- und Streckendaten");

        public static readonly string Help = T._("Dokumentation zum GTFS-Export");
        public static readonly string HelpLink = T._("https://fahrplan.manuelhu.de/gtfs/");
    }
}