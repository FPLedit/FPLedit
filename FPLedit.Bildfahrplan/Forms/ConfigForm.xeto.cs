﻿using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Linq;
using FPLedit.Shared.UI;
using System.Drawing;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class ConfigForm : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private DropDown stationFontComboBox, timeColorComboBox, trainColorComboBox, stationColorComboBox, bgColorComboBox, timeFontComboBox, trainFontComboBox, trainWidthComboBox;
        private DropDown stationFontSizeComboBox, timeFontSizeComboBox, trainFontSizeComboBox, stationWidthComboBox, hourTimeWidthComboBox, minuteTimeWidthComboBox;
        private TextBox heightPerHourTextBox;
        private TextBox startTimeTextBox;
        private TextBox endTimeTextBox;
        private CheckBox includeKilometreCheckBox, drawStationNamesCheckBox, stationLinesCheckBox, stationVerticalCheckBox;
#pragma warning restore CS0649
        private NumberValidator heightPerHourValidator;

        private Timetable tt;
        private TimetableStyle attrs;

        private ConfigForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            heightPerHourValidator = new NumberValidator(heightPerHourTextBox, false, true);
            heightPerHourValidator.ErrorMessage = "Bitte eine Zahl als Höhe pro Stunde angeben!";

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

            heightPerHourTextBox.TextBinding.AddIntConvBinding<TimetableStyle, TextControl>(s => s.HeightPerHour);

            string convFromTs(TimeSpan ts) => ts.ToShortTimeString();
            TimeSpan convToTs(string s) => TimeSpan.Parse(s.Replace("24:", "1.00:"));
            startTimeTextBox.TextBinding.Convert(convToTs, convFromTs).BindDataContext<TimetableStyle>(s => s.StartTime);
            endTimeTextBox.TextBinding.Convert(convToTs, convFromTs).BindDataContext<TimetableStyle>(s => s.EndTime);

            includeKilometreCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DisplayKilometre);
            drawStationNamesCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.DrawHeader);
            stationVerticalCheckBox.CheckedBinding.BindDataContext<TimetableStyle>(s => s.StationVertical);

            this.AddCloseHandler();
        }

        public ConfigForm(Timetable tt, ISettings settings) : this(settings)
        {
            this.tt = tt;
            attrs = new TimetableStyle(tt);
            DataContext = attrs;

            stationLinesCheckBox.Checked = attrs.StationLines != StationLineStyle.None;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!heightPerHourValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            attrs.StationLines = stationLinesCheckBox.Checked.Value ? StationLineStyle.Normal : StationLineStyle.None;

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
