using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FPLedit.Shared;

namespace FPLedit.Standard
{
    public partial class TrainsEditForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        public TrainsEditForm()
        {
            InitializeComponent();

            InitListView(topListView);
            InitListView(bottomListView);
        }

        public TrainsEditForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            topLineLabel.Text = "Züge " + tt.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + tt.GetLineName(BOTTOM_DIRECTION);
            UpdateListView(topListView, TOP_DIRECTION);
            UpdateListView(bottomListView, BOTTOM_DIRECTION);

            KeyDown += (s, e) =>
            {
                ListView active = null;
                TrainDirection dir = default(TrainDirection);

                if (ActiveControl == topListView)
                {
                    active = topListView;
                    dir = TOP_DIRECTION;
                }
                if (ActiveControl == bottomListView)
                {
                    active = bottomListView;
                    dir = BOTTOM_DIRECTION;
                }

                if (active == null)
                    return;

                if (e.KeyCode == Keys.Delete)
                    DeleteTrain(active, dir, false);
                else if (e.KeyCode == Keys.B && e.Control)
                    EditTrain(active, dir, false);
                else if (e.KeyCode == Keys.N && e.Control)
                    NewTrain(active, dir);
            };
        }

        private void UpdateListView(ListView view, TrainDirection direction)
        {
            view.Items.Clear();
            foreach (var train in tt.Trains.Where(o => o.Direction == direction))
            {
                view.Items.Add(new ListViewItem(new[] {
                    train.TName,
                    train.Locomotive,
                    train.Mbr,
                    train.Last,
                    DaysHelper.DaysToString(train.Days),
                    train.Comment })
                { Tag = train });
            }
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void InitListView(ListView view)
        {
            view.Columns.Add("Zugnummer");
            view.Columns.Add("Tfz");
            view.Columns.Add("Mbr");
            view.Columns.Add("Last");
            view.Columns.Add("Verkehrstage");
            view.Columns.Add("Kommentar");
        }

        private void DeleteTrain(ListView view, TrainDirection direction, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                tt.RemoveTrain((Train)item.Tag);

                UpdateListView(view, direction);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        private void EditTrain(ListView view, TrainDirection direction, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                Train train = (Train)item.Tag;

                TrainEditForm tef = new TrainEditForm(train);
                if (tef.ShowDialog() == DialogResult.OK)
                {
                    UpdateListView(view, direction);
                    var changedItem = view.Items.OfType<ListViewItem>().Where(i => i.Tag == train).First();
                    changedItem.Selected = true;
                    changedItem.EnsureVisible();
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        private void NewTrain(ListView view, TrainDirection direction)
        {
            TrainEditForm tef = new TrainEditForm(info.Timetable, direction);
            if (tef.ShowDialog() == DialogResult.OK)
            {
                Train tra = tef.Train;
                foreach (var sta in tt.Stations)
                    tra.AddArrDep(sta, new ArrDep());
                tt.AddTrain(tra);

                UpdateListView(view, direction);
                var changedItem = view.Items.OfType<ListViewItem>().Where(i => i.Tag == tra).First();
                changedItem.Selected = true;
                changedItem.EnsureVisible();
            }
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

        private void bottomListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(bottomListView, BOTTOM_DIRECTION, false);

        private void topListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(topListView, TOP_DIRECTION, false);
    }
}
