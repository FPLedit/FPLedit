using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan.BildfahrplanExport
{
    public partial class StationVelocityForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private string velocity = "";

        public StationVelocityForm()
        {
            InitializeComponent();

            listView.Columns.Add("Station");
            listView.Columns.Add("Vmax");
        }

        public void Init(IInfo info)
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();
            UpdateStations();
        }

        private void UpdateStations()
        {
            listView.Items.Clear();
            foreach (var station in tt.Stations)
            {
                listView.Items.Add(new ListViewItem(new[] {
                    station.Name,
                    station.GetMeta("MaxVelocity", velocity),
                }) { Tag = station });
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void EditVmax(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];
                Station train = tt.Stations[tt.Stations.IndexOf((Station)item.Tag)];

                StationVelocityEditForm vef = new StationVelocityEditForm();
                vef.Initialize(train);
                if (vef.ShowDialog() == DialogResult.OK)
                    UpdateStations();                
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Höchstgeschwindigkeit ändern");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();
            Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            info.ClearBackup();
            Close();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditVmax();

        private void trainListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditVmax(false);
    }
}
