using Eto.Forms;
using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport
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

        public ConfigForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            heightPerHourValidator = new NumberValidator(heightPerHourTextBox, false, true);
            heightPerHourValidator.ErrorMessage = "Bitte eine Zahl als Höhe pro Stunde angeben!";

            var cc = new ColorCollection();

            bgColorComboBox.DataStore = cc.ColorHexStrings;
            stationColorComboBox.DataStore = cc.ColorHexStrings;
            timeColorComboBox.DataStore = cc.ColorHexStrings;
            trainColorComboBox.DataStore = cc.ColorHexStrings;
            bgColorComboBox.ItemTextBinding = cc.ColorBinding;
            stationColorComboBox.ItemTextBinding = cc.ColorBinding;
            timeColorComboBox.ItemTextBinding = cc.ColorBinding;
            trainColorComboBox.ItemTextBinding = cc.ColorBinding;

            var fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name);
            stationFontComboBox.DataStore = fontFamilies;
            timeFontComboBox.DataStore = fontFamilies;
            trainFontComboBox.DataStore = fontFamilies;

            var fontSizes = new object[11];
            for (int i = 5; i <= 15; i++) fontSizes[i - 5] = i;
            stationFontSizeComboBox.DataStore = fontSizes;
            timeFontSizeComboBox.DataStore = fontSizes;
            trainFontSizeComboBox.DataStore = fontSizes;

            var lineWidths = new object[5];
            for (int i = 1; i <= 5; i++) lineWidths[i - 1] = i;
            trainWidthComboBox.DataStore = lineWidths;
            hourTimeWidthComboBox.DataStore = lineWidths;
            minuteTimeWidthComboBox.DataStore = lineWidths;
            stationWidthComboBox.DataStore = lineWidths;
        }

        public ConfigForm(Timetable tt) : this()
        {
            this.tt = tt;
            attrs = new TimetableStyle(tt);
            bgColorComboBox.SelectedValue = ColorHelper.ToHexString(attrs.BgColor);
            timeColorComboBox.SelectedValue = ColorHelper.ToHexString(attrs.TimeColor);
            trainColorComboBox.SelectedValue = ColorHelper.ToHexString(attrs.TrainColor);
            stationColorComboBox.SelectedValue = ColorHelper.ToHexString(attrs.StationColor);

            hourTimeWidthComboBox.SelectedValue = attrs.HourTimeWidth;
            minuteTimeWidthComboBox.SelectedValue = attrs.MinuteTimeWidth;
            trainWidthComboBox.SelectedValue = attrs.TrainWidth;
            stationWidthComboBox.SelectedValue = attrs.StationWidth;

            stationFontComboBox.SelectedValue = attrs.StationFont.FontFamily.Name;
            timeFontComboBox.SelectedValue = attrs.TimeFont.FontFamily.Name;
            trainFontComboBox.SelectedValue = attrs.TrainFont.FontFamily.Name;
            stationFontSizeComboBox.SelectedValue = (int)attrs.StationFont.Size;
            timeFontSizeComboBox.SelectedValue = (int)attrs.TimeFont.Size;
            trainFontSizeComboBox.SelectedValue = (int)attrs.TrainFont.Size;

            stationLinesCheckBox.Checked = attrs.StationLines;
            includeKilometreCheckBox.Checked = attrs.DisplayKilometre;
            drawStationNamesCheckBox.Checked = attrs.DrawHeader;
            stationVerticalCheckBox.Checked = attrs.StationVertical;

            heightPerHourTextBox.Text = attrs.HeightPerHour.ToString();
            startTimeTextBox.Text = attrs.StartTime.ToShortTimeString();
            endTimeTextBox.Text = attrs.EndTime.ToShortTimeString();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!heightPerHourValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            attrs.BgColor = ColorHelper.FromHexString((string)bgColorComboBox.SelectedValue);
            attrs.TimeColor = ColorHelper.FromHexString((string)timeColorComboBox.SelectedValue);
            attrs.TrainColor = ColorHelper.FromHexString((string)trainColorComboBox.SelectedValue);
            attrs.StationColor = ColorHelper.FromHexString((string)stationColorComboBox.SelectedValue);

            attrs.HourTimeWidth = (int)hourTimeWidthComboBox.SelectedValue;
            attrs.MinuteTimeWidth = (int)minuteTimeWidthComboBox.SelectedValue;
            attrs.TrainWidth = (int)trainWidthComboBox.SelectedValue;
            attrs.StationWidth = (int)stationWidthComboBox.SelectedValue;

            attrs.StationFont = new Font((string)stationFontComboBox.SelectedValue, (int)stationFontSizeComboBox.SelectedValue);
            attrs.TimeFont = new Font((string)timeFontComboBox.SelectedValue, (int)timeFontSizeComboBox.SelectedValue);
            attrs.TrainFont = new Font((string)trainFontComboBox.SelectedValue, (int)trainFontSizeComboBox.SelectedValue);

            attrs.StationLines = stationLinesCheckBox.Checked.Value;
            attrs.DisplayKilometre = includeKilometreCheckBox.Checked.Value;
            attrs.DrawHeader = drawStationNamesCheckBox.Checked.Value;
            attrs.StationVertical = stationVerticalCheckBox.Checked.Value;

            attrs.HeightPerHour = int.Parse(heightPerHourTextBox.Text);
            attrs.StartTime = TimeSpan.Parse(startTimeTextBox.Text.Replace("24:", "1.00:"));
            attrs.EndTime = TimeSpan.Parse(endTimeTextBox.Text.Replace("24:", "1.00:"));

            Result = DialogResult.Ok;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }
    }
}
