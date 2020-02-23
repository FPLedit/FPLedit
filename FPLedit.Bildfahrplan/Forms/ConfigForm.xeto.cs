
using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Linq;
using FPLedit.Shared.UI;
using System.Collections.Generic;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class ConfigForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown stationFontComboBox, timeColorComboBox, trainColorComboBox, stationColorComboBox, bgColorComboBox, timeFontComboBox, trainFontComboBox, trainWidthComboBox;
        private readonly DropDown stationFontSizeComboBox, timeFontSizeComboBox, trainFontSizeComboBox, stationWidthComboBox, hourTimeWidthComboBox, minuteTimeWidthComboBox;
        private readonly DropDown stationLinesDropDown;
        private readonly TextBox heightPerHourTextBox;
        private readonly TextBox startTimeTextBox;
        private readonly TextBox endTimeTextBox;
        private readonly CheckBox includeKilometreCheckBox, drawStationNamesCheckBox, stationVerticalCheckBox, multitrackCheckBox, networkTrainsCheckBox;
#pragma warning restore CS0649
        private readonly NumberValidator heightPerHourValidator;
        private readonly TimeValidator startTimeValidator, endTimeValidator;
        private readonly ValidatorCollection validators;

        private readonly TimetableStyle attrs;
        private readonly Timetable tt;

        public ConfigForm(Timetable tt, ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.tt = tt;

            heightPerHourValidator = new NumberValidator(heightPerHourTextBox, false, false, errorMessage: "Bitte eine Zahl als Höhe pro Stunde angeben!");
            startTimeValidator = new TimeValidator(startTimeTextBox, false, errorMessage: "Bitte eine gültige Uhrzeit im Format hh:mm angeben!");
            endTimeValidator = new TimeValidator(endTimeTextBox, false, errorMessage: "Bitte eine gültige Uhrzeit im Format hh:mm angeben!");
            validators = new ValidatorCollection(heightPerHourValidator, startTimeValidator, endTimeValidator);

            DropDownBind.Color<TimetableStyle>(settings, bgColorComboBox, "BgColor");
            DropDownBind.Color<TimetableStyle>(settings, stationColorComboBox, "StationColor");
            DropDownBind.Color<TimetableStyle>(settings, timeColorComboBox, "TimeColor");
            DropDownBind.Color<TimetableStyle>(settings, trainColorComboBox, "TrainColor");

            DropDownBind.Font<TimetableStyle>(stationFontComboBox, stationFontSizeComboBox, "StationFont");
            DropDownBind.Font<TimetableStyle>(timeFontComboBox, timeFontSizeComboBox, "TimeFont");
            DropDownBind.Font<TimetableStyle>(trainFontComboBox, trainFontSizeComboBox, "TrainFont");

            DropDownBind.Width<TimetableStyle>(hourTimeWidthComboBox, "HourTimeWidth");
            DropDownBind.Width<TimetableStyle>(minuteTimeWidthComboBox, "MinuteTimeWidth");
            DropDownBind.Width<TimetableStyle>(stationWidthComboBox, "StationWidth");
            DropDownBind.Width<TimetableStyle>(trainWidthComboBox, "TrainWidth");

            var styles = new Dictionary<StationLineStyle, string>()
            {
                [StationLineStyle.None] = "Keine",
                [StationLineStyle.Normal] = "Gerade Linien",
                [StationLineStyle.Cubic] = "Kubische Linien",
            };
            if (tt.Version == TimetableVersion.JTG2_x)
                styles.Remove(StationLineStyle.Cubic);
            DropDownBind.Enum<TimetableStyle, StationLineStyle>(stationLinesDropDown, "StationLines", styles);

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
                MessageBox.Show("Bitte erst alle Fehler beheben: " + Environment.NewLine + validators.Message);
                return;
            }

            Result = DialogResult.Ok;
            this.NClose();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            this.NClose();
        }

        private void CalcTimesButton_Click(object sender, EventArgs e)
        {
            var times = tt.Trains.SelectMany(t => t.GetArrDeps().Values).SelectMany(a =>
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
    }
}
