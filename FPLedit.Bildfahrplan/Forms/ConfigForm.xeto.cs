
using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Linq;
using FPLedit.Shared.UI;
using System.Collections.Generic;

namespace FPLedit.Bildfahrplan.Forms;

internal sealed class ConfigForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly DropDown stationFontComboBox = default!, timeColorComboBox = default!, trainColorComboBox = default!, stationColorComboBox = default!, bgColorComboBox = default!, timeFontComboBox = default!, trainFontComboBox = default!, trainWidthComboBox = default!;
    private readonly DropDown stationFontSizeComboBox = default!, timeFontSizeComboBox = default!, trainFontSizeComboBox = default!, stationWidthComboBox = default!, hourTimeWidthComboBox = default!, minuteTimeWidthComboBox = default!;
    private readonly DropDown stationLinesDropDown = default!;
    private readonly TextBox heightPerHourTextBox = default!;
    private readonly TextBox startTimeTextBox = default!;
    private readonly TextBox endTimeTextBox = default!;
    private readonly CheckBox includeKilometreCheckBox = default!, drawStationNamesCheckBox = default!, stationVerticalCheckBox = default!, multitrackCheckBox = default!, networkTrainsCheckBox = default!;
#pragma warning restore CS0649,CA2213
    private readonly ValidatorCollection validators;

    private readonly TimetableStyle attrs;
    private readonly Timetable tt;
    private readonly IPluginInterface pluginInterface;
    private readonly object backupHandle;

    public ConfigForm(Timetable tt, IPluginInterface pluginInterface)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        backupHandle = pluginInterface.BackupTimetable();

        this.tt = tt;
        this.pluginInterface = pluginInterface;

        var heightPerHourValidator = new NumberValidator(heightPerHourTextBox, false, false, errorMessage: T._("Bitte eine Zahl als Höhe pro Stunde angeben!"));
        var startTimeValidator = new TimeValidator(startTimeTextBox, false, errorMessage: T._("Bitte eine gültige Uhrzeit im Format hh:mm angeben!"));
        var endTimeValidator = new TimeValidator(endTimeTextBox, false, errorMessage: T._("Bitte eine gültige Uhrzeit im Format hh:mm angeben!"), maximum: new TimeEntry(48, 0));
        validators = new ValidatorCollection(heightPerHourValidator, startTimeValidator, endTimeValidator);

        DropDownBind.Color<TimetableStyle>(pluginInterface.Settings, bgColorComboBox, nameof(TimetableStyle.BgColor));
        DropDownBind.Color<TimetableStyle>(pluginInterface.Settings, stationColorComboBox, nameof(TimetableStyle.StationColor));
        DropDownBind.Color<TimetableStyle>(pluginInterface.Settings, timeColorComboBox, nameof(TimetableStyle.TimeColor));
        DropDownBind.Color<TimetableStyle>(pluginInterface.Settings, trainColorComboBox, nameof(TimetableStyle.TrainColor));

        DropDownBind.Font<TimetableStyle>(stationFontComboBox, stationFontSizeComboBox, nameof(TimetableStyle.StationFont));
        DropDownBind.Font<TimetableStyle>(timeFontComboBox, timeFontSizeComboBox, nameof(TimetableStyle.TimeFont));
        DropDownBind.Font<TimetableStyle>(trainFontComboBox, trainFontSizeComboBox, nameof(TimetableStyle.TrainFont));

        DropDownBind.Width<TimetableStyle>(hourTimeWidthComboBox, nameof(TimetableStyle.HourTimeWidth));
        DropDownBind.Width<TimetableStyle>(minuteTimeWidthComboBox, nameof(TimetableStyle.MinuteTimeWidth));
        DropDownBind.Width<TimetableStyle>(stationWidthComboBox, nameof(TimetableStyle.StationWidth));
        DropDownBind.Width<TimetableStyle>(trainWidthComboBox, nameof(TimetableStyle.TrainWidth));

        var styles = new Dictionary<StationLineStyle, string>()
        {
            [StationLineStyle.None] = T._("Keine"),
            [StationLineStyle.Normal] = T._("Gerade Linien"),
            [StationLineStyle.Cubic] = T._("Kubische Linien"),
        };
        DropDownBind.Enum<TimetableStyle, StationLineStyle>(stationLinesDropDown, nameof(TimetableStyle.StationLines), styles);

        heightPerHourTextBox.TextBinding.AddFloatConvBinding<TimetableStyle, TextControl>(s => s.HeightPerHour);

        startTimeTextBox.TextBinding.AddTimeEntryConvBinding<TimetableStyle, TextControl>(s => s.StartTime);
        endTimeTextBox.TextBinding.AddTimeEntryConvBinding<TimetableStyle, TextControl>(s => s.EndTime);

        includeKilometreCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DisplayKilometre);
        drawStationNamesCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DrawHeader);
        stationVerticalCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.StationVertical);
        multitrackCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.MultiTrack);
        networkTrainsCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DrawNetworkTrains);

        networkTrainsCheckBox.Enabled = tt.Type == TimetableType.Network;

        attrs = new TimetableStyle(tt);
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

        if (attrs.StartTime > attrs.EndTime)
        {
            MessageBox.Show(T._("Die Startzeit muss vor der Endzeit liegen!"));
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

    private void CalcTimesButton_Click(object sender, EventArgs e)
    {
        var times = tt.Trains.SelectMany(t => t.GetArrDepsUnsorted().Values).SelectMany(a =>
        {
            var ts = a.ShuntMoves.Select(sm => sm.Time).ToList();
            ts.AddRange(new[] { a.Arrival, a.Departure });
            return ts;
        }).Where(t => t != default);

        var offset = new TimeEntry(0, 5);
        var max = new TimeEntry(24, 0);
        var start = times.DefaultIfEmpty().Min();
        var end = times.DefaultIfEmpty(max).Max();
        attrs.StartTime = start > default(TimeEntry) + offset ? start - offset : default;
        attrs.EndTime = end < max - offset ? end + offset : max;

        startTimeTextBox.UpdateBindings(BindingUpdateMode.Destination);
        endTimeTextBox.UpdateBindings(BindingUpdateMode.Destination);
    }

    private static class L
    {
        public static readonly string BackgroundColor = T._("Hintergundfarbe");
        public static readonly string StationColor = T._("Bahnhofslinienfarbe, -stärke");
        public static readonly string TimeColor = T._("Zeitlinienfarbe");
        public static readonly string HourTimeWidth = T._("Zeitlinienstärke zur vollen Stunde");
        public static readonly string TimeWidth = T._("Zeitlinienstärke");
        public static readonly string TrainColorWidth = T._("Zuglinienfarbe, -stärke");
        public static readonly string StationFont = T._("Bahnhofsschriftart, -größe");
        public static readonly string TimeFont = T._("Zeitenschriftart, -größe");
        public static readonly string TrainFont = T._("Zugnummernschriftart, -größe");
        public static readonly string StationLines = T._("Linien für stehende Züge in Bahnhöfen");
        public static readonly string ShowKilometers = T._("Streckenkilometer anzeigen");
        public static readonly string ShowStationNames = T._("Stationsnamen anzeigen");
        public static readonly string StationsVertical = T._("Stationsnamen vertikal zeichnen");
        public static readonly string Multitrack = T._("Gleise zeichnen");
        public static readonly string NetworkTrains = T._("An Knotenbahnhöfen Züge anderer Strecken zeichnen");
        public static readonly string HeightPerHour = T._("Höhe pro Stunde");
        public static readonly string StartTime = T._("Startzeit");
        public static readonly string EndTime = T._("Endzeit");
        public static readonly string CalcTimes = T._("Zeiten berechnen");
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Darstellung ändern");
    }
}