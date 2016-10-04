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

            topLineLabel.Text = "Züge " + tt.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + tt.GetLineName(BOTTOM_DIRECTION);
            UpdateListView(topListView, TOP_DIRECTION);
            UpdateListView(bottomListView, BOTTOM_DIRECTION);
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
            InitListView(topListView);
            InitListView(bottomListView);
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

        private void EditTrain(ListView view, bool direction, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.Items[view.SelectedIndices[0]];
                Train train = tt.Trains[tt.Trains.IndexOf((Train)item.Tag)];

                TrainEditForm tef = new TrainEditForm();
                tef.Initialize(train);
                if (tef.ShowDialog() == DialogResult.OK)
                    UpdateListView(view, direction);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        private void NewTrain(ListView view, bool direction)
        {
            TrainEditForm tef = new TrainEditForm();
            tef.Initialize(direction);
            if (tef.ShowDialog() == DialogResult.OK)
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
                if (mef.ShowDialog() == DialogResult.OK)
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

        private void topNewButton_Click(object sender, EventArgs e)
            => NewTrain(topListView, TOP_DIRECTION);

        private void topEditButton_Click(object sender, EventArgs e)
            => EditTrain(topListView, TOP_DIRECTION);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(topListView, TOP_DIRECTION);

        private void bottomNewButton_Click(object sender, EventArgs e)
            => NewTrain(bottomListView, BOTTOM_DIRECTION);

        private void bottomEditButton_Click(object sender, EventArgs e)
            => EditTrain(bottomListView, BOTTOM_DIRECTION);

        private void bottomDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(bottomListView, BOTTOM_DIRECTION);

        private void topEditButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                EditMeta(topListView, TOP_DIRECTION);
        }

        private void bottomEditButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                EditMeta(bottomListView, BOTTOM_DIRECTION);
        }

        private void bottomListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(bottomListView, BOTTOM_DIRECTION, false);

        private void topListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(topListView, TOP_DIRECTION, false);
    }
}
