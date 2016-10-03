using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Buchfahrplan.Shared;

namespace Buchfahrplan.Standard
{
    public partial class TrainsEditForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private const bool TOP_DIRECTION = false;
        private const bool BOTTOM_DIRECTION = true;

        public TrainsEditForm()
        {
            InitializeComponent();
        }

        public void Init(IInfo info)
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            topFromToLabel.Text = "Züge " + tt.GetLineName(TOP_DIRECTION);
            bottomFromToLabel.Text = "Züge " + tt.GetLineName(BOTTOM_DIRECTION);
            UpdateListView(topTrainListView, TOP_DIRECTION);
            UpdateListView(bottomTrainListView, BOTTOM_DIRECTION);
        }

        private void UpdateListView(ListView view, bool direction)
        {
            view.Items.Clear();
            foreach (var train in tt.Trains.Where(o => o.Direction == direction))
            {
                view.Items.Add(new ListViewItem(new[] {
                    train.Name,
                    train.Line,
                    train.Locomotive,
                    train.DaysToString() })
                { Tag = train });
            }
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void InitListView(ListView view)
        {
            view.Columns.Add("Zugnummer");
            view.Columns.Add("Strecke");
            view.Columns.Add("Tfz");
            view.Columns.Add("Verkehrstage");            
        }

        private void TrainsEditForm_Load(object sender, EventArgs e)
        {
            InitListView(topTrainListView);
            InitListView(bottomTrainListView);
        }

        private void DeleteTrain(ListView view, bool direction)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.Items[view.SelectedIndices[0]];
                tt.Trains.Remove((Train)item.Tag);

                UpdateListView(view, direction);
            }
            else
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        private void EditTrain(ListView view, bool direction)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.Items[view.SelectedIndices[0]];
                Train train = tt.Trains[tt.Trains.IndexOf((Train)item.Tag)];

                TrainEditForm tef = new TrainEditForm();
                tef.Initialize(train);
                DialogResult res = tef.ShowDialog();
                if (res == DialogResult.OK)
                    UpdateListView(view, direction);
            }
            else
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        private void NewTrain(ListView view, bool direction)
        {
            TrainEditForm tef = new TrainEditForm();
            tef.Initialize(direction);
            DialogResult res = tef.ShowDialog();
            if (res == DialogResult.OK)
            {
                Train tra = tef.NewTrain;
                tra.InitializeStations(tt);
                tt.Trains.Add(tra);

                UpdateListView(view, direction);
            }
        }

        private void EditMeta(ListView view, bool direction)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.Items[view.SelectedIndices[0]];
                Train train = tt.Trains[tt.Trains.IndexOf((Train)item.Tag)];

                MetaEdit mef = new MetaEdit();
                mef.Initialize(train);
                DialogResult res = mef.ShowDialog();
                if (res == DialogResult.OK)
                    UpdateListView(view, direction);
            }
            else
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug-Metadaten bearbeiten");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();
            Close();
        } 

        private void topNewTrainButton_Click(object sender, EventArgs e)
            => NewTrain(topTrainListView, TOP_DIRECTION);

        private void topEditTrainButton_Click(object sender, EventArgs e)
            => EditTrain(topTrainListView, TOP_DIRECTION);

        private void topDeleteTrainButton_Click(object sender, EventArgs e)
            => DeleteTrain(topTrainListView, TOP_DIRECTION);

        private void bottomNewTrainButton_Click(object sender, EventArgs e)
            => NewTrain(bottomTrainListView, BOTTOM_DIRECTION);

        private void bottomEditTrainButton_Click(object sender, EventArgs e)
            => EditTrain(bottomTrainListView, BOTTOM_DIRECTION);

        private void bottomDeleteTrainButton_Click(object sender, EventArgs e)
            => DeleteTrain(bottomTrainListView, BOTTOM_DIRECTION);

        private void topEditTrainButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                EditMeta(topTrainListView, TOP_DIRECTION);
        }

        private void bottomEditTrainButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                EditMeta(bottomTrainListView, BOTTOM_DIRECTION);
        }
    }
}
