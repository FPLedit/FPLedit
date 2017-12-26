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

namespace FPLedit.Editor.Network
{
    public partial class TrainsEditingForm : Form
    {
        private IInfo info;
        private Timetable tt;

        private TrainsEditingForm()
        {
            InitializeComponent();
            InitListView(listView);
        }

        public TrainsEditingForm(IInfo info) : this()
        {
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            UpdateListView(listView);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    DeleteTrain(listView, false);
                else if ((e.KeyCode == Keys.T && e.Control))
                    EditTimetable(listView);
                else if ((e.KeyCode == Keys.C && e.Control))
                    CopyTrain(listView);
                else if ((e.KeyCode == Keys.B && e.Control) || (e.KeyCode == Keys.Enter))
                    EditTrain(listView, false);
                else if (e.KeyCode == Keys.N && e.Control)
                    NewTrain(listView);
            };
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
            view.Columns.Add("Laufweg");
            view.Columns.Add("Kommentar");
        }

        private ListViewItem CreateItem(Train t)
        {
            var path = t.GetPath();
            return new ListViewItem(new[] {
                t.TName,
                t.Locomotive,
                t.Mbr,
                t.Last,
                DaysHelper.DaysToString(t.Days),
                path.FirstOrDefault()?.SName + " - " + path.LastOrDefault()?.SName,
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

        private void CopyTrain(ListView view, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var train = (Train)view.SelectedItems[0].Tag;

                var tcf = new TrainCopyDialog(train, info.Timetable);
                tcf.ShowDialog();

                UpdateListView(view);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
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
            => NewTrain(listView);

        private void topEditButton_Click(object sender, EventArgs e)
            => EditTrain(listView);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(listView);

        private void editTimetableButton_Click(object sender, EventArgs e)
            => EditTimetable(listView);

        private void copyButton_Click(object sender, EventArgs e)
            => CopyTrain(listView);
    }
}
