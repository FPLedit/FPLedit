using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared.UI;

namespace FPLedit.DebugDump.Forms
{
    internal class InspectForm : FDialog<DialogResult>
    {
        private readonly DumpEvent[] events;
        private DumpEvent currentEvent;

#pragma warning disable CS0649
        private readonly ListBox eventListBox;
        private readonly TextArea propertyTextArea, dataTextArea;
        private readonly DropDown dataDropDown;
#pragma warning restore CS0649

        public InspectForm(DumpEvent[] events)
        {
            this.events = events;

            Eto.Serialization.Xaml.XamlReader.Load(this);

            eventListBox.ItemTextBinding = Binding.Property<DumpEvent, string>(evt => evt.Time.ToString("g") + " " + evt.Type);
            eventListBox.SelectedIndexChanged += EventListBoxOnSelectedIndexChanged;
            eventListBox.DataStore = events;
            dataDropDown.ItemTextBinding = Binding.Delegate<int, string>(i => i > -1 ? "Data [" + i + "]" : "All Data");
            dataDropDown.SelectedIndexChanged += DataDropDownOnSelectedIndexChanged;
        }

        private void DataDropDownOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentEvent == null || dataDropDown.SelectedValue == null)
                return;

            var idx = (int) dataDropDown.SelectedValue;
            if (idx >= 0)
                dataTextArea.Text = currentEvent.Data[idx];
            else
                dataTextArea.Text = string.Join("\n\n\n", currentEvent.Data);
        }

        private void EventListBoxOnSelectedIndexChanged(object sender, EventArgs e)
        {
            currentEvent = (DumpEvent) eventListBox.SelectedValue;
            if (currentEvent == null)
                return;
            propertyTextArea.Text = "Time: " + currentEvent.Time.ToString("g") + "\nType: " + currentEvent.Type + "\nData count: " + currentEvent.Data.Length;
            dataDropDown.DataStore = Enumerable.Range(-1, currentEvent.Data.Length + 1).Cast<object>();
            if (currentEvent.Data.Any())
                dataDropDown.SelectedIndex = 0;
        }

        private void CloseButton_Click(object sender, EventArgs e) => Close();

        private void SaveFile(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                if (sfd.ShowDialog(this) == DialogResult.Ok)
                {
                    File.WriteAllText(sfd.FileName, dataTextArea.Text);
                }
            }
        }
    }
}