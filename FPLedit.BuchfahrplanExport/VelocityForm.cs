using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class VelocityForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private string velocity = "";

        public VelocityForm()
        {
            InitializeComponent();

            listView.Columns.Add("km");
            listView.Columns.Add("Name");
            listView.Columns.Add("Vmax");
        }

        public VelocityForm(IInfo info) : this()
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
                    station.Kilometre.ToString(),
                    station.SName,
                    station.GetAttribute<string>("fpl-vmax", velocity),
                }) { Tag = station });                
            }

            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");                
            if (attrsEn != null)
            {
                var attrs = new BFPL_Attrs(attrsEn,tt);
                foreach (var point in attrs.Points.OrderBy(p => p.Kilometre))
                {
                    listView.Items.Add(new ListViewItem(new[] {
                    point.Kilometre.ToString(),
                    point.PName,
                    point.GetAttribute<string>("fpl-vmax", velocity),
                })
                    { Tag = point });
                }
            }            

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void EditVmax(bool message = true)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem item = listView.Items[listView.SelectedIndices[0]];

                if (item.GetType() == typeof(Station))
                {
                    Station station = tt.Stations[tt.Stations.IndexOf((Station)item.Tag)];

                    VelocityEditForm vef = new VelocityEditForm(station);
                    if (vef.ShowDialog() == DialogResult.OK)
                    {
                        UpdateStations();
                        var changedItem = listView.Items.OfType<ListViewItem>().Where(i => i.Tag == station).First();
                        changedItem.Selected = true;
                        changedItem.EnsureVisible();
                    }
                }

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
