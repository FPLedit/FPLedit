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
    public partial class ConfigForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown stationFontComboBox, timeColorComboBox, trainColorComboBox, stationColorComboBox, bgColorComboBox, timeFontComboBox, trainFontComboBox, trainWidthComboBox;
        private readonly DropDown stationFontSizeComboBox, timeFontSizeComboBox, trainFontSizeComboBox, stationWidthComboBox, hourTimeWidthComboBox, minuteTimeWidthComboBox;
        private readonly DropDown stationLinesDropDown;
        private readonly TextBox heightPerHourTextBox;
        private readonly TextBox startTimeTextBox;
        private readonly TextBox endTimeTextBox;
        private readonly CheckBox includeKilometreCheckBox, drawStationNamesCheckBox, stationVerticalCheckBox;
#pragma warning restore CS0649
        private readonly NumberValidator heightPerHourValidator;
        private readonly TimeValidator startTimeValidator, endTimeValidator;
        private readonly ValidatorCollection validators;

        private TimetableStyle attrs;

        public ConfigForm(Timetable tt, ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            heightPerHourValidator = new NumberValidator(heightPerHourTextBox, false, false);
            heightPerHourValidator.ErrorMessage = "Bitte eine Zahl als Höhe pro Stunde angeben!";
            startTimeValidator = new TimeValidator(startTimeTextBox, false);
            endTimeValidator = new TimeValidator(endTimeTextBox, false);
            startTimeValidator.ErrorMessage = endTimeValidator.ErrorMessage = "Bitte eine gültige Uhrzeit im Format hh:mm angeben!";
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

            startTimeTextBox.TextBinding.AddTimeSpanConvBinding<TimetableStyle, TextControl>(s => s.StartTime);
            endTimeTextBox.TextBinding.AddTimeSpanConvBinding<TimetableStyle, TextControl>(s => s.EndTime);

            includeKilometreCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DisplayKilometre);
            drawStationNamesCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DrawHeader);
            stationVerticalCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.StationVertical);

            attrs = new TimetableStyle(tt);
            DataContext = attrs;

            this.AddCloseHandler();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!validators.IsValid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben: " + Environment.NewLine + validators.Message);
                return;
            }

            Result = DialogResult.Ok;
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            this.NClose();
        }
    }
}
