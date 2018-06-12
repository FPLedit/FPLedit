using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    internal class TrainsEditingForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;

#pragma warning disable CS0649
        private GridView gridView;
#pragma warning restore CS0649

        public TrainsEditingForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            tt = info.Timetable;
            info.BackupTimetable();

            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.TName) },
                HeaderText = "Zugnummer"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Locomotive) },
                HeaderText = "Tfz"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Mbr) },
                HeaderText = "Mbr"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Last) },
                HeaderText = "Last"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => DaysHelper.DaysToString(t.Days, false)) },
                HeaderText = "Verkehrstage"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => BuildPath(t)) },
                HeaderText = "Laufweg"
            });
            gridView.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<Train, string>(t => t.Comment) },
                HeaderText = "Kommentar"
            });

            gridView.MouseDoubleClick += (s, e) => EditTrain(gridView, false);

            UpdateListView(gridView);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;
            else
                gridView.KeyDown += HandleKeystroke;
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
                DeleteTrain(gridView, false);
            else if ((e.Key == Keys.T && e.Control))
                EditTimetable(gridView, false);
            else if ((e.Key == Keys.C && e.Control))
                CopyTrain(gridView, false);
            else if ((e.Key == Keys.P && e.Control))
                EditPath(gridView, false);
            else if ((e.Key == Keys.B && e.Control) || (e.Key == Keys.Enter))
                EditTrain(gridView, false);
            else if (e.Key == Keys.N && e.Control)
                NewTrain(gridView);
        }

        private string BuildPath(Train t)
        {
            var path = t.GetPath();
            return path.FirstOrDefault()?.SName + " - " + path.LastOrDefault()?.SName;
        }

        private void UpdateListView(GridView view)
        {
            view.DataStore = tt.Trains;
        }

        private void DeleteTrain(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                tt.RemoveTrain((Train)view.SelectedItem);

                UpdateListView(gridView);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        private void EditTrain(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                Train train = (Train)view.SelectedItem;

                TrainEditForm tef = new TrainEditForm(train);
                if (tef.ShowModal(this) == DialogResult.Ok)
                    UpdateListView(view);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        private void EditTimetable(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train)view.SelectedItem;

                TrainTimetableEditor tte = new TrainTimetableEditor(info, train);
                tte.ShowModal(this);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug-Fahrplan bearbeiten");
        }

        private void EditPath(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train)view.SelectedItem;

                TrainChangeRouteForm trf = new TrainChangeRouteForm(info, train);
                if (trf.ShowModal(this) == DialogResult.Ok)
                    UpdateListView(view);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug-Fahrplan bearbeiten");
        }

        private void CopyTrain(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train)view.SelectedItem;

                var tcf = new TrainCopyDialog(train, info.Timetable);
                tcf.ShowModal(this);

                UpdateListView(view);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }

        private void NewTrain(GridView view)
        {
            var trf = new TrainChangeRouteForm(info);
            if (trf.ShowModal(this) != DialogResult.Ok)
                return;

            TrainEditForm tef = new TrainEditForm(info.Timetable, TrainDirection.tr);
            if (tef.ShowModal(this) == DialogResult.Ok)
            {
                tef.Train.AddAllArrDeps(trf.Path);
                tt.AddTrain(tef.Train);

                UpdateListView(view);
            }
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

        private void topNewButton_Click(object sender, EventArgs e)
            => NewTrain(gridView);

        private void topEditButton_Click(object sender, EventArgs e)
            => EditTrain(gridView);

        private void topDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(gridView);

        private void editTimetableButton_Click(object sender, EventArgs e)
            => EditTimetable(gridView);

        private void copyButton_Click(object sender, EventArgs e)
            => CopyTrain(gridView);

        private void editPathButton_Click(object sender, EventArgs e)
            => EditPath(gridView);
    }
}
