using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;

namespace FPLedit.Editor.Linear
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
                    DeleteTrain(active, false);
                else if ((e.KeyCode == Keys.B && e.Control) || (e.KeyCode == Keys.Enter))
                    EditTrain(active, false);
                else if (e.KeyCode == Keys.N && e.Control)
                    NewTrain(active, dir);
            };
        }

        private void UpdateListView(ListView view, TrainDirection direction)
        {
            view.Items.Clear();
            foreach (var train in tt.Trains.Where(o => o.Direction == direction))
                view.Items.Add(CreateItem(train));

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

        private void DeleteTrain(ListView view, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                tt.RemoveTrain((Train)item.Tag);

                view.Items.Remove(item);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        private void EditTrain(ListView view, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                Train train = (Train)item.Tag;

                TrainEditForm tef = new TrainEditForm(train);
                if (tef.ShowDialog() == DialogResult.OK)
                {
                    item.SubItems[0].Text = train.TName;
                    item.SubItems[1].Text = train.Locomotive;
                    item.SubItems[2].Text = train.Mbr;
                    item.SubItems[3].Text = train.Last;
                    item.SubItems[4].Text = DaysHelper.DaysToString(train.Days);
                    item.SubItems[5].Text = train.Comment;

                    view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
                tt.AddTrain(tef.Train);

                var item = view.Items.Add(CreateItem(tef.Train));

                item.Selected = true;
                item.EnsureVisible();
            }
        }

        private void CopyTrain(ListView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var train = (Train)view.SelectedItems[0].Tag;

                var tcf = new TrainCopyDialog(train, info.Timetable);
                tcf.ShowDialog();

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }

        private ListViewItem CreateItem(Train t)
        {
            return new ListViewItem(new[] {
                t.TName,
                t.Locomotive,
                t.Mbr,
                t.Last,
                DaysHelper.DaysToString(t.Days),
                t.Comment
            }) { Tag = t };
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
            => EditTrain(topListView);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(topListView);

        private void bottomNewButton_Click(object sender, EventArgs e)
            => NewTrain(bottomListView, BOTTOM_DIRECTION);

        private void bottomEditButton_Click(object sender, EventArgs e)
            => EditTrain(bottomListView);

        private void bottomDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(bottomListView);

        private void bottomListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(bottomListView, false);

        private void topListView_MouseDoubleClick(object sender, MouseEventArgs e)
            => EditTrain(topListView, false);

        private void topCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(topListView, TOP_DIRECTION, true);

        private void bottomCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(bottomListView, BOTTOM_DIRECTION, true);
    }
}
