using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class StationStyleForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;
        private TimetableStyle attrs;
        private ColorCollection cc;
        private DashStyleHelper ds;

#pragma warning disable CS0649
        private GridView gridView;
#pragma warning restore CS0649

        public StationStyleForm(IInfo info)
        {
            this.info = info;
            tt = info.Timetable;
            attrs = new TimetableStyle(tt);
            info.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(info.Settings);
            ds = new DashStyleHelper();

            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Station, string>(t => t.SName) },
                HeaderText = "Name"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Station, string>(t => cc.ToName(new StationStyle(t).StationColor ?? attrs.StationColor)) },
                HeaderText = "Farbe"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Station, string>(t => (new StationStyle(t).StationWidth ?? attrs.StationWidth).ToString()) },
                HeaderText = "Linienstärke"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Station, string>(t => (ds.GetDescription(new StationStyle(t).LineStyle))) },
                HeaderText = "Linientyp"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new CheckBoxCell { Binding = Binding.Property<Station, bool?>(t => new StationStyle(t).Show) },
                HeaderText = "Station zeichnen"
            });

            gridView.CellDoubleClick += (s, e) => EditColor(false);

            UpdateStations();
        }

        private void UpdateStations()
        {
            gridView.DataStore = tt.Stations;
        }

        private void EditColor(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                var station = (Station)gridView.SelectedItem;

                var sef = new StationStyleEditForm(station, info.Settings);
                if (sef.ShowModal(this) == DialogResult.Ok)
                    UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Stationsdarstellung ändern");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();
            Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;
            info.ClearBackup();
            Close();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditColor();
    }
}
