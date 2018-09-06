using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
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

        public TrainColorForm(IInfo info)
        {
            this.info = info;
            tt = info.Timetable;
            attrs = new TimetableStyle(tt);
            info.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(info.Settings);
            ds = new DashStyleHelper();

            gridView.AddColumn<Train>(t => t.TName, "Zugnummer");
            gridView.AddColumn<Train>(t => cc.ToName(new TrainStyle(t).TrainColor ?? attrs.TrainColor), "Farbe");
            gridView.AddColumn<Train>(t => (new TrainStyle(t).TrainWidth ?? attrs.TrainWidth).ToString(), "Linienstärke");
            gridView.AddColumn<Train>(t => ds.GetDescription(new TrainStyle(t).LineStyle), "Linientyp");
            gridView.AddColumn<Train>(t => new TrainStyle(t).Show.ToString(), "Zug zeichnen");

            gridView.CellDoubleClick += (s, e) => EditColor(false);

            UpdateTrains();

            this.AddCloseHandler();
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

                TrainColorEditForm tcef = new TrainColorEditForm(train, info.Settings);
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
            this.NClose();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;
            info.ClearBackup();
            this.NClose();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditColor();
    }
}
