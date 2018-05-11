using Eto.Forms;
using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport.Forms
{
    public partial class TrainColorForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;
        private TimetableStyle attrs;
        private ColorCollection cc;
        private DashStyleHelper ds;

#pragma warning disable CS0649
        private GridView gridView;
#pragma warning restore CS0649

        public TrainColorForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection();
            ds = new DashStyleHelper();

            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.TName) },
                HeaderText = "Zugnummer"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => cc.ToName(new TrainStyle(t).TrainColor ?? attrs.TrainColor)) },
                HeaderText = "Farbe"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => (new TrainStyle(t).TrainWidth ?? attrs.TrainWidth).ToString()) },
                HeaderText = "Linienstärke"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => (ds.GetDescription(new TrainStyle(t).LineStyle))) },
                HeaderText = "Linientyp"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => new TrainStyle(t).Show.ToString()) },
                HeaderText = "Zug zeichnen"
            });

            gridView.CellDoubleClick += (s, e) => EditColor(false);
        }

        public TrainColorForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            attrs = new TimetableStyle(tt);
            info.BackupTimetable();

            UpdateTrains();
        }

        private void UpdateTrains()
        {
            gridView.DataStore = tt.Trains;
        }

        private void EditColor(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                var train = (Train)gridView.SelectedItem;

                TrainColorEditForm tcef = new TrainColorEditForm(train);
                if (tcef.ShowModal(this) == DialogResult.Ok)
                    UpdateTrains();
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zugdarstellung ändern");
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
