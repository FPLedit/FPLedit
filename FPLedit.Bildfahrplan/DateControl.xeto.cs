using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using FPLedit.BildfahrplanExport.Model;
using Eto.Forms;

namespace FPLedit.BildfahrplanExport
{
    public partial class DateControl : Panel
    {
        private Timetable tt;
        private TimetableStyle attrs;

        public event EventHandler ValueChanged;

#pragma warning disable CS0649
        private CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
#pragma warning restore CS0649
        private CheckBox[] daysBoxes;

        public DateControl(Timetable tt)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };

            this.tt = tt;
            attrs = new TimetableStyle(tt);

            for (int i = 0; i < attrs.RenderDays.Length; i++)
            {
                daysBoxes[i].Checked = attrs.RenderDays[i];
                daysBoxes[i].CheckedChanged += CheckBox_CheckedChanged;
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var days = new bool[7];
            for (int i = 0; i < daysBoxes.Length; i++)
                days[i] = daysBoxes[i].Checked.Value;
            attrs.RenderDays = days;

            ValueChanged?.Invoke(this, new EventArgs());
        }

        private void preferencesButton_Click(object sender, EventArgs e)
        {
            ConfigForm cnf = new ConfigForm(tt);
            cnf.ShowModal(Parent);
            ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}
