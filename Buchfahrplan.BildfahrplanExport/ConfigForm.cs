using Buchfahrplan.Shared;
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

namespace Buchfahrplan.BildfahrplanExport
{
    public partial class ConfigForm : Form
    {
        private Timetable tt;

        public ConfigForm()
        {
            InitializeComponent();

            bgColorComboBox.Items.AddRange(colors);
            stationColorComboBox.Items.AddRange(colors);
            timeColorComboBox.Items.AddRange(colors);
            trainColorComboBox.Items.AddRange(colors);

            string[] fontFamilies = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            stationFontComboBox.Items.AddRange(fontFamilies);
            timeFontComboBox.Items.AddRange(fontFamilies);
            trainFontComboBox.Items.AddRange(fontFamilies);

            var fontSizes = new string[11];
            for (int i = 5; i <= 15; i++) fontSizes[i - 5] = i.ToString();
            stationFontSizeComboBox.Items.AddRange(fontSizes);
            timeFontSizeComboBox.Items.AddRange(fontSizes);
            trainFontSizeComboBox.Items.AddRange(fontSizes);

            var lineWidths = new string[5];
            for (int i = 1; i <= 5; i++) lineWidths[i - 1] = i.ToString();
            trainWidthComboBox.Items.AddRange(lineWidths);
            hourTimeWidthComboBox.Items.AddRange(lineWidths);
            minuteTimeWidthComboBox.Items.AddRange(lineWidths);
            stationWidthComboBox.Items.AddRange(lineWidths);

            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            bgColorComboBox.SelectedIndex = 2;
            stationColorComboBox.SelectedIndex = 0;
            timeColorComboBox.SelectedIndex = 4;
            trainColorComboBox.SelectedIndex = 0;
            stationFontComboBox.SelectedItem = "Arial";
            timeFontComboBox.SelectedItem = "Arial";
            trainFontComboBox.SelectedItem = "Arial";
            stationFontSizeComboBox.SelectedItem = "9";
            timeFontSizeComboBox.SelectedItem = "9";            
            trainFontSizeComboBox.SelectedItem = "9";
            trainWidthComboBox.SelectedIndex = 0;
            hourTimeWidthComboBox.SelectedIndex = 1;
            minuteTimeWidthComboBox.SelectedIndex = 0;
            stationWidthComboBox.SelectedIndex = 0;
            stationLinesCheckBox.Checked = true;
            includeKilometreCheckBox.Checked = true;
            drawStationNamesCheckBox.Checked = true;
            heightPerHourTextBox.Text = "150";
            startTimeTextBox.Text = "00:00";
            endTimeTextBox.Text = "24:00";
        }

        public ConfigForm(Timetable tt) : this()
        {
            this.tt = tt;
            bgColorComboBox.SelectedItem = tt.GetMeta("BgColor", (string)bgColorComboBox.SelectedItem);
            timeColorComboBox.SelectedItem = tt.GetMeta("TimeColor", (string)timeColorComboBox.SelectedItem);
            trainColorComboBox.SelectedItem = tt.GetMeta("TrainColor", (string)trainColorComboBox.SelectedItem);
            stationColorComboBox.SelectedItem = tt.GetMeta("StationColor", (string)stationColorComboBox.SelectedItem);
            hourTimeWidthComboBox.SelectedItem = tt.GetMeta("HourTimeWidth", (string)hourTimeWidthComboBox.SelectedItem);
            minuteTimeWidthComboBox.SelectedItem = tt.GetMeta("MinuteTimeWidth", (string)minuteTimeWidthComboBox.SelectedItem);
            trainWidthComboBox.SelectedItem = tt.GetMeta("TrainWidth", (string)trainWidthComboBox.SelectedItem);
            stationWidthComboBox.SelectedItem = tt.GetMeta("StationWidth", (string)stationWidthComboBox.SelectedItem);

            stationFontComboBox.SelectedItem = tt.GetMeta("StationFont", (string)stationFontComboBox.SelectedItem);
            timeFontComboBox.SelectedItem = tt.GetMeta("TimeFont", (string)timeFontComboBox.SelectedItem);
            trainFontComboBox.SelectedItem = tt.GetMeta("TrainFont", (string)trainFontComboBox.SelectedItem);
            stationFontSizeComboBox.SelectedItem = tt.GetMeta("StationFontSize", (string)stationFontSizeComboBox.SelectedItem);
            timeFontSizeComboBox.SelectedItem = tt.GetMeta("TimeFontSize", (string)timeFontSizeComboBox.SelectedItem);
            trainFontSizeComboBox.SelectedItem = tt.GetMeta("TrainFontSize", (string)trainFontSizeComboBox.SelectedItem);

            stationLinesCheckBox.Checked = tt.GetMetaBool("StationLines", stationLinesCheckBox.Checked);
            includeKilometreCheckBox.Checked = tt.GetMetaBool("DisplayKilometre", includeKilometreCheckBox.Checked);
            drawStationNamesCheckBox.Checked = tt.GetMetaBool("DrawHeader", drawStationNamesCheckBox.Checked);

            heightPerHourTextBox.Text = tt.GetMeta("HeightPerHour", heightPerHourTextBox.Text);
            startTimeTextBox.Text = tt.GetMeta("StartTime", startTimeTextBox.Text);
            endTimeTextBox.Text = tt.GetMeta("EndTime", endTimeTextBox.Text);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            tt.Metadata["BgColor"] = (string)bgColorComboBox.SelectedItem;

            tt.Metadata["TimeColor"] = (string)timeColorComboBox.SelectedItem;
            tt.Metadata["TrainColor"] = (string)trainColorComboBox.SelectedItem;
            tt.Metadata["StationColor"] = (string)stationColorComboBox.SelectedItem;
            tt.Metadata["HourTimeWidth"] = (string)hourTimeWidthComboBox.SelectedItem;
            tt.Metadata["MinuteTimeWidth"] = (string)minuteTimeWidthComboBox.SelectedItem;
            tt.Metadata["TrainWidth"] = (string)trainWidthComboBox.SelectedItem;
            tt.Metadata["StationWidth"] = (string)stationWidthComboBox.SelectedItem;

            tt.Metadata["StationFont"] = (string)stationFontComboBox.SelectedItem;
            tt.Metadata["TimeFont"] = (string)timeFontComboBox.SelectedItem;
            tt.Metadata["TrainFont"] = (string)trainFontComboBox.SelectedItem;
            tt.Metadata["StationFontSize"] = (string)stationFontSizeComboBox.SelectedItem;
            tt.Metadata["TimeFontSize"] = (string)timeFontSizeComboBox.SelectedItem;
            tt.Metadata["TrainFontSize"] = (string)trainFontSizeComboBox.SelectedItem;

            tt.Metadata["StationLines"] = stationLinesCheckBox.Checked.ToString();
            tt.Metadata["DisplayKilometre"] = includeKilometreCheckBox.Checked.ToString();
            tt.Metadata["DrawHeader"] = drawStationNamesCheckBox.Checked.ToString();

            tt.Metadata["HeightPerHour"] = heightPerHourTextBox.Text;
            tt.Metadata["StartTime"] = startTimeTextBox.Text;
            tt.Metadata["EndTime"] = endTimeTextBox.Text;
        }

        private string[] colors = new[]
        {
            "Black",
            "Gray",
            "White",
            "Red",
            "Orange",
            "Yellow",
            "Blue",
            "LightBlue",
            "Green",
            "DrakGreen",
            "Brown",
            "Magenta"
        };

        private void defaultValuesButton_Click(object sender, EventArgs e)
        {
            SetDefaultValues();
        }
    }
}
