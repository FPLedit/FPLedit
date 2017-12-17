using FPLedit.Editor;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.NewEditor
{
    public partial class TrainsEditingForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private TrainsEditingForm()
        {
            InitializeComponent();
            InitListView(topListView);
        }

        public TrainsEditingForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            UpdateListView(topListView);
        }

        private void UpdateListView(ListView view)
        {
            view.Items.Clear();
            foreach (var train in tt.Trains)
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

        private ListViewItem CreateItem(Train t)
        {
            return new ListViewItem(new[] {
                    t.TName,
                    t.Locomotive,
                    t.Mbr,
                    t.Last,
                    DaysHelper.DaysToString(t.Days),
                    t.Comment })
            { Tag = t };
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

        private void EditTimetable(ListView view, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var train = (Train)view.SelectedItems[0].Tag;

                TrainTimetableEditor tte = new TrainTimetableEditor(info, train);
                tte.ShowDialog();
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug-Fahrplan bearbeiten");
        }

        private void NewTrain(ListView view)
        {
            var trf = new TrainSelectRouteForm(info);
            if (trf.ShowDialog() != DialogResult.OK)
                return;

            TrainEditForm tef = new TrainEditForm(info.Timetable, TrainDirection.tr);
            if (tef.ShowDialog() == DialogResult.OK)
            {
                tef.Train.AddAllArrDeps(trf.TrainRoute);
                tt.AddTrain(tef.Train);

                var item = view.Items.Add(CreateItem(tef.Train));

                item.Selected = true;
                item.EnsureVisible();
            }
        }

        private void TrainsEditingForm_Load(object sender, EventArgs e)
        {

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
            => NewTrain(topListView);

        private void topEditButton_Click(object sender, EventArgs e)
            => EditTrain(topListView);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(topListView);

        private void editTimetableButton_Click(object sender, EventArgs e)
            => EditTimetable(topListView);
    }
}
