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
        public event EventHandler ValueChanged;

#pragma warning disable CS0649
        private readonly CheckBox mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox;
#pragma warning restore CS0649
        private readonly CheckBox[] daysBoxes;
        private TimetableStyle attrs;

        public DateControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };
        }

        public void Initialize(IPluginInterface pluginInterface)
        {
            attrs = new TimetableStyle(pluginInterface.Timetable);

            for (int i = 0; i < attrs.RenderDays.Length; i++)
            {
                daysBoxes[i].Checked = attrs.RenderDays[i];
                daysBoxes[i].CheckedChanged += CheckBox_CheckedChanged;
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var days = daysBoxes.Select(b => b.Checked.Value).ToArray();
            attrs.RenderDays = new Days(days);

            ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}
