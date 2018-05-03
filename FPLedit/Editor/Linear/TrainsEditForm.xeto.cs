using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Linear
{
    internal class TrainsEditForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;

#pragma warning disable CS0649
        private GridView topGridView, bottomGridView;
        private Label topLineLabel, bottomLineLabel;
#pragma warning restore CS0649

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        public TrainsEditForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            InitListView(topGridView);
            InitListView(bottomGridView);

            topLineLabel.Text = "Züge " + tt.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + tt.GetLineName(BOTTOM_DIRECTION);
            UpdateListView(topGridView, TOP_DIRECTION);
            UpdateListView(bottomGridView, BOTTOM_DIRECTION);

            bottomGridView.CellDoubleClick += (s, e) => EditTrain(bottomGridView, BOTTOM_DIRECTION, false);
            topGridView.CellDoubleClick += (s, e) => EditTrain(topGridView, TOP_DIRECTION, false);

            KeyDown += (s, e) =>
            {
                GridView active = null;
                TrainDirection dir = default(TrainDirection);

                if (topGridView.HasFocus)
                {
                    active = topGridView;
                    dir = TOP_DIRECTION;
                }
                if (bottomGridView.HasFocus)
                {
                    active = bottomGridView;
                    dir = BOTTOM_DIRECTION;
                }

                if (active == null)
                    return;

                if (e.Key == Keys.Delete)
                    DeleteTrain(active, dir, false);
                else if ((e.Key == Keys.B && e.Control) || (e.Key == Keys.Enter))
                    EditTrain(active, dir, false);
                else if (e.Key == Keys.N && e.Control)
                    NewTrain(active, dir);
            };
        }

        private void UpdateListView(GridView view, TrainDirection direction)
        {
            view.DataStore = tt.Trains.Where(t => t.Direction == direction);
        }

        private void InitListView(GridView view)
        {
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.TName) },
                HeaderText = "Zugnummer"
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Locomotive) },
                HeaderText = "Tfz"
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Mbr) },
                HeaderText = "Mbr"
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Last) },
                HeaderText = "Last"
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => DaysHelper.DaysToString(t.Days, false)) },
                HeaderText = "Verkehrstage"
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Comment) },
                HeaderText = "Kommentar"
            });
        }

        private void DeleteTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                tt.RemoveTrain((Train)view.SelectedItem);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        private void EditTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                Train train = (Train)view.SelectedItem;

                TrainEditForm tef = new TrainEditForm(train);
                if (tef.ShowModal(this) == DialogResult.Ok)
                    UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        private void NewTrain(GridView view, TrainDirection direction)
        {
            TrainEditForm tef = new TrainEditForm(info.Timetable, direction);
            if (tef.ShowModal(this) == DialogResult.Ok)
            {
                tt.AddTrain(tef.Train);

                UpdateListView(view, direction);
            }
        }

        private void CopyTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train)view.SelectedItem;

                var tcf = new TrainCopyDialog(train, info.Timetable);
                tcf.ShowModal(this);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            Result = DialogResult.Ok;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();
            Close();
        }

        #region Events
        private void topNewButton_Click(object sender, EventArgs e)
            => NewTrain(topGridView, TOP_DIRECTION);

        private void topEditButton_Click(object sender, EventArgs e)
            => EditTrain(topGridView, TOP_DIRECTION);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(topGridView, TOP_DIRECTION);

        private void bottomNewButton_Click(object sender, EventArgs e)
            => NewTrain(bottomGridView, BOTTOM_DIRECTION);

        private void bottomEditButton_Click(object sender, EventArgs e)
            => EditTrain(bottomGridView, BOTTOM_DIRECTION);

        private void bottomDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(bottomGridView, BOTTOM_DIRECTION);

        private void topCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(topGridView, TOP_DIRECTION, true);

        private void bottomCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(bottomGridView, BOTTOM_DIRECTION, true);
        #endregion
    }
}
