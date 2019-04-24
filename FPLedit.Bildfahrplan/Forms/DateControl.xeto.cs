using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using FPLedit.Bildfahrplan.Model;
using Eto.Forms;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class DateControl : Panel
    {
        private TimetableStyle attrs;

        public event EventHandler ValueChanged;

#pragma warning disable CS0649
        private CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
#pragma warning restore CS0649
        private CheckBox[] daysBoxes;

        public DateControl(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };

            attrs = new TimetableStyle(info.Timetable);

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
            attrs.RenderDays = new Days(days);

            ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}
