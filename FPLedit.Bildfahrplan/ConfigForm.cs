using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.BildfahrplanExport
{
    public partial class ConfigForm : Form
    {
        private Timetable tt;
        private TimetableStyle attrs;

        public ConfigForm()
        {
            InitializeComponent();

            bgColorComboBox.Items.AddRange(ColorHelper.ColorNames);
            stationColorComboBox.Items.AddRange(ColorHelper.ColorNames);
            timeColorComboBox.Items.AddRange(ColorHelper.ColorNames);
            trainColorComboBox.Items.AddRange(ColorHelper.ColorNames);

            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            stationFontComboBox.Items.AddRange(fontFamilies);
            timeFontComboBox.Items.AddRange(fontFamilies);
            trainFontComboBox.Items.AddRange(fontFamilies);

            var fontSizes = new object[11];
            for (int i = 5; i <= 15; i++) fontSizes[i - 5] = i;
            stationFontSizeComboBox.Items.AddRange(fontSizes);
            timeFontSizeComboBox.Items.AddRange(fontSizes);
            trainFontSizeComboBox.Items.AddRange(fontSizes);

            var lineWidths = new object[5];
            for (int i = 1; i <= 5; i++) lineWidths[i - 1] = i;
            trainWidthComboBox.Items.AddRange(lineWidths);
            hourTimeWidthComboBox.Items.AddRange(lineWidths);
            minuteTimeWidthComboBox.Items.AddRange(lineWidths);
            stationWidthComboBox.Items.AddRange(lineWidths);
        }

        public ConfigForm(Timetable tt) : this()
        {
            this.tt = tt;
            attrs = new TimetableStyle(tt);
            bgColorComboBox.SelectedItem = ColorHelper.NameFromColor(attrs.BgColor);
            timeColorComboBox.SelectedItem = ColorHelper.NameFromColor(attrs.TimeColor);
            trainColorComboBox.SelectedItem = ColorHelper.NameFromColor(attrs.TrainColor);
            stationColorComboBox.SelectedItem = ColorHelper.NameFromColor(attrs.StationColor);

            hourTimeWidthComboBox.SelectedItem = attrs.HourTimeWidth;
            minuteTimeWidthComboBox.SelectedItem = attrs.MinuteTimeWidth;
            trainWidthComboBox.SelectedItem = attrs.TrainWidth;
            stationWidthComboBox.SelectedItem = attrs.StationWidth;

            stationFontComboBox.SelectedItem = attrs.StationFont.FontFamily.Name;
            timeFontComboBox.SelectedItem = attrs.TimeFont.FontFamily.Name;
            trainFontComboBox.SelectedItem = attrs.TrainFont.FontFamily.Name;
            stationFontSizeComboBox.SelectedItem = (int)attrs.StationFont.Size;
            timeFontSizeComboBox.SelectedItem = (int)attrs.TimeFont.Size;
            trainFontSizeComboBox.SelectedItem = (int)attrs.TrainFont.Size;

            stationLinesCheckBox.Checked = attrs.StationLines;
            includeKilometreCheckBox.Checked = attrs.DisplayKilometre;
            drawStationNamesCheckBox.Checked = attrs.DrawHeader;

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

            attrs.BgColor = ColorHelper.ColorFromName((string)bgColorComboBox.SelectedItem);
            attrs.TimeColor = ColorHelper.ColorFromName((string)timeColorComboBox.SelectedItem);
            attrs.TrainColor = ColorHelper.ColorFromName((string)trainColorComboBox.SelectedItem);
            attrs.StationColor = ColorHelper.ColorFromName((string)stationColorComboBox.SelectedItem);

            attrs.HourTimeWidth = (int)hourTimeWidthComboBox.SelectedItem;
            attrs.MinuteTimeWidth = (int)minuteTimeWidthComboBox.SelectedItem;
            attrs.TrainWidth = (int)trainWidthComboBox.SelectedItem;
            attrs.StationWidth = (int)stationWidthComboBox.SelectedItem;

            attrs.StationFont = new Font((string)stationFontComboBox.SelectedItem, (int)stationFontSizeComboBox.SelectedItem);
            attrs.TimeFont = new Font((string)timeFontComboBox.SelectedItem, (int)timeFontSizeComboBox.SelectedItem);
            attrs.TrainFont = new Font((string)trainFontComboBox.SelectedItem, (int)trainFontSizeComboBox.SelectedItem);

            attrs.StationLines = stationLinesCheckBox.Checked;
            attrs.DisplayKilometre = includeKilometreCheckBox.Checked;
            attrs.DrawHeader = drawStationNamesCheckBox.Checked;

            attrs.HeightPerHour = int.Parse(heightPerHourTextBox.Text);
            attrs.StartTime = TimeSpan.Parse(startTimeTextBox.Text.Replace("24:", "1.00:"));
            attrs.EndTime = TimeSpan.Parse(endTimeTextBox.Text.Replace("24:", "1.00:"));
        }
    }
}
