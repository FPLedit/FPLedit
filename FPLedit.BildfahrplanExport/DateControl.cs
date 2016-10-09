using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FPLedit.Shared;

namespace FPLedit.BildfahrplanExport
{
    public partial class DateControl : UserControl
    {
        private Timetable tt;

        public event EventHandler ValueChanged;

        public DateControl()
        {
            InitializeComponent();
        }

        public DateControl(Timetable tt): this()
        {
            this.tt = tt;
            if (tt.Metadata.ContainsKey("ShowDays"))
            {
                var days = Train.ParseDays(tt.Metadata["ShowDays"]);
                mondayCheckBox.Checked = days[0];
                tuesdayCheckBox.Checked = days[1];
                wednesdayCheckBox.Checked = days[2];
                thursdayCheckBox.Checked = days[3];
                fridayCheckBox.Checked = days[4];
                saturdayCheckBox.Checked = days[5];
                sundayCheckBox.Checked = days[6];
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            tt.Metadata["ShowDays"] = Train.DaysToBinString(new[] { mondayCheckBox.Checked,
                tuesdayCheckBox.Checked, wednesdayCheckBox.Checked,
                thursdayCheckBox.Checked, fridayCheckBox.Checked,
                saturdayCheckBox.Checked, sundayCheckBox.Checked});
            ValueChanged?.Invoke(this, new EventArgs());
        }

        private void preferencesButton_Click(object sender, EventArgs e)
        {
            ConfigForm cnf = new ConfigForm(tt);
            cnf.ShowDialog();
            ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}
