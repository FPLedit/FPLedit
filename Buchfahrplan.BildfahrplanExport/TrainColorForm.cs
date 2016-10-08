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

namespace FPLedit.BildfahrplanExport
{
    public partial class TrainColorForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private string trainColor = "Black";
        private string trainWidth = "1";
        private string drawTrain = "True";

        public TrainColorForm()
        {
            InitializeComponent();

            trainListView.Columns.Add("Zugnummer");
            trainListView.Columns.Add("Farbe");
            trainListView.Columns.Add("Linienstärke");
            trainListView.Columns.Add("Zug zeichnen");
        }

        public TrainColorForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            trainColor = tt.GetMeta("TrainColor", trainColor);
            trainWidth = tt.GetMeta("TrainWidth", trainWidth);
            UpdateTrains();
        }

        private void UpdateTrains()
        {
            trainListView.Items.Clear();
            foreach (var train in tt.Trains)
            {
                trainListView.Items.Add(new ListViewItem(new[] {
                    train.Name,
                    train.GetMeta("Color", trainColor),
                    train.GetMeta("Width", trainWidth),
                    train.GetMeta("Draw", drawTrain)})
                { Tag = train });
            }

            trainListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            trainListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void EditColor(bool message = true)
        {
            if (trainListView.SelectedItems.Count > 0)
            {
                ListViewItem item = trainListView.Items[trainListView.SelectedIndices[0]];
                Train train = tt.Trains[tt.Trains.IndexOf((Train)item.Tag)];

                TrainColorEditForm tcef = new TrainColorEditForm(train);
                if (tcef.ShowDialog() == DialogResult.OK)
                    UpdateTrains();
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zugdarstellung ändern");
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
            => EditColor();

        private void trainListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditColor(false);
    }
}
