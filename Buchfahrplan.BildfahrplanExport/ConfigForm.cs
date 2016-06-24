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
            heightPerHourTextBox.Text = "150";
            startTimeTextBox.Text = "00:00";
            endTimeTextBox.Text = "24:00";
        }

        public void Init(Timetable tt)
        {
            this.tt = tt;
            if (tt.Metadata.ContainsKey("BgColor")) bgColorComboBox.SelectedItem = tt.Metadata["BgColor"];
            
            if (tt.Metadata.ContainsKey("TimeColor")) timeColorComboBox.SelectedItem = tt.Metadata["TimeColor"];
            if (tt.Metadata.ContainsKey("TrainColor")) trainColorComboBox.SelectedItem = tt.Metadata["TrainColor"];
            if (tt.Metadata.ContainsKey("StationColor")) stationColorComboBox.SelectedItem = tt.Metadata["StationColor"];
            if (tt.Metadata.ContainsKey("HourTimeWidth")) hourTimeWidthComboBox.SelectedItem = tt.Metadata["HourTimeWidth"];
            if (tt.Metadata.ContainsKey("MinuteTimeWidth")) minuteTimeWidthComboBox.SelectedItem = tt.Metadata["MinuteTimeWidth"];
            if (tt.Metadata.ContainsKey("TrainWidth")) trainWidthComboBox.SelectedItem = tt.Metadata["TrainWidth"];
            if (tt.Metadata.ContainsKey("StationWidth")) stationWidthComboBox.SelectedItem = tt.Metadata["StationWidth"];

            if (tt.Metadata.ContainsKey("StationFont")) stationFontComboBox.SelectedItem = tt.Metadata["StationFont"];
            if (tt.Metadata.ContainsKey("TimeFont")) timeFontComboBox.SelectedItem = tt.Metadata["TimeFont"];
            if (tt.Metadata.ContainsKey("TrainFont")) trainFontComboBox.SelectedItem = tt.Metadata["TrainFont"];
            if (tt.Metadata.ContainsKey("StationFontSize")) stationFontSizeComboBox.SelectedItem = tt.Metadata["StationFontSize"];
            if (tt.Metadata.ContainsKey("TimeFontSize")) timeFontSizeComboBox.SelectedItem = tt.Metadata["TimeFontSize"];
            if (tt.Metadata.ContainsKey("TrainFontSize")) trainFontSizeComboBox.SelectedItem = tt.Metadata["TrainFontSize"];

            if (tt.Metadata.ContainsKey("StationLines")) stationLinesCheckBox.Checked = bool.Parse(tt.Metadata["StationLines"]);
            if (tt.Metadata.ContainsKey("StationLines")) stationLinesCheckBox.Checked = bool.Parse(tt.Metadata["StationLines"]);

            if (tt.Metadata.ContainsKey("HeightPerHour")) heightPerHourTextBox.Text = tt.Metadata["HeightPerHour"];
            if (tt.Metadata.ContainsKey("StartTime")) startTimeTextBox.Text = tt.Metadata["StartTime"];
            if (tt.Metadata.ContainsKey("EndTime")) endTimeTextBox.Text = tt.Metadata["EndTime"];
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
