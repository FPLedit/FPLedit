using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Data;
using Font = System.Drawing.Font;
using System.Linq;
using FPLedit.Shared.Helpers;
using FPLedit.Shared.UI;
using System.Reflection;
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

            //TODO: Refactor??
            PrepareColorComboBox(settings, new Entry(bgColorComboBox, "BgColor"), new Entry(stationColorComboBox, "StationColor"),
                 new Entry(timeColorComboBox, "TimeColor"), new Entry(trainColorComboBox, "TrainColor"));

            var fontFamilies = FontCollection.Families;
            stationFontComboBox.DataStore = fontFamilies;
            timeFontComboBox.DataStore = fontFamilies;
            trainFontComboBox.DataStore = fontFamilies;

            var fontSizes = new object[11];
            for (int i = 5; i <= 15; i++) fontSizes[i - 5] = i;
            stationFontSizeComboBox.DataStore = fontSizes;
            timeFontSizeComboBox.DataStore = fontSizes;
            trainFontSizeComboBox.DataStore = fontSizes;

            PrepareWidthDropDowns(new Entry(trainWidthComboBox, "TrainWidth"), new Entry(hourTimeWidthComboBox, "HourTimeWidth"),
                new Entry(minuteTimeWidthComboBox, "MinuteTimeWidth"), new Entry(stationWidthComboBox, "StationWidth"));

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

            stationFontComboBox.SelectedValue = attrs.StationFont.FontFamily.Name;
            timeFontComboBox.SelectedValue = attrs.TimeFont.FontFamily.Name;
            trainFontComboBox.SelectedValue = attrs.TrainFont.FontFamily.Name;
            stationFontSizeComboBox.SelectedValue = (int)attrs.StationFont.Size;
            timeFontSizeComboBox.SelectedValue = (int)attrs.TimeFont.Size;
            trainFontSizeComboBox.SelectedValue = (int)attrs.TrainFont.Size;

            stationLinesCheckBox.Checked = attrs.StationLines != StationLineStyle.None;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!heightPerHourValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            attrs.StationFont = new Font((string)stationFontComboBox.SelectedValue, (int)stationFontSizeComboBox.SelectedValue);
            attrs.TimeFont = new Font((string)timeFontComboBox.SelectedValue, (int)timeFontSizeComboBox.SelectedValue);
            attrs.TrainFont = new Font((string)trainFontComboBox.SelectedValue, (int)trainFontSizeComboBox.SelectedValue);

            attrs.StationLines = stationLinesCheckBox.Checked.Value ? StationLineStyle.Normal : StationLineStyle.None;

            Result = DialogResult.Ok;
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            this.NClose();
        }

        private void PrepareColorComboBox(ISettings settings, params Entry[] boxes)
        {
            var cc = new ColorCollection(settings);
            foreach (var box in boxes)
            {
                var b = box;
                b.DropDown.DataStore = cc.ColorHexStrings;
                b.DropDown.ItemTextBinding = cc.ColorBinding;
                b.DropDown.SelectedValueBinding.BindDataContext<TimetableStyle>(a => ColorFormatter.ToString((Color)b.Property.GetValue(a)),
                    (a, val) => b.Property.SetValue(a, ColorFormatter.FromHexString((string)val)));
            }
        }

        private void PrepareWidthDropDowns(params Entry[] boxes)
        {
            var lineWidths = Enumerable.Range(1, 5).Cast<object>().ToArray();
            foreach (var box in boxes)
            {
                var b = box;
                b.DropDown.DataStore = lineWidths;
                b.DropDown.SelectedValueBinding.BindDataContext<TimetableStyle>(a => (int)b.Property.GetValue(a),
                    (a, val) => b.Property.SetValue(a, (int)val));
            }
        }

        private class Entry
        {
            public DropDown DropDown { get; set; }

            public PropertyInfo Property { get; set; }

            public Entry(DropDown control, string property)
            {
                DropDown = control;
                Property = typeof(TimetableStyle).GetProperty(property);
            }
        }
    }
}
