using Eto.Forms;
using FPLedit.Editor.Trains;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;

namespace FPLedit.Editor.Network
{
    internal sealed class NetworkTrainsEditForm : BaseTrainsEditor
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly object backupHandle;

#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button editPathButton, editButton, deleteButton, copyButton;
#pragma warning restore CS0649

        public NetworkTrainsEditForm(IPluginInterface pluginInterface) : base(pluginInterface.Timetable)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            tt = pluginInterface.Timetable;
            backupHandle = pluginInterface.BackupTimetable();

            gridView.AddColumn<ITrain>(t => t.IsLink ? "L" : "", "");
            gridView.AddColumn<ITrain>(t => t.TName, "Zugnummer");
            gridView.AddColumn<ITrain>(t => t.Locomotive, "Tfz");
            gridView.AddColumn<ITrain>(t => t.Mbr, "Mbr");
            gridView.AddColumn<ITrain>(t => t.Last, "Last");
            gridView.AddColumn<ITrain>(t => t.Days.DaysToString(false), "Verkehrstage");
            gridView.AddColumn<ITrain>(t => BuildPath(t), "Laufweg");
            gridView.AddColumn<ITrain>(t => t.Comment, "Kommentar");

            gridView.MouseDoubleClick += (s, e) => EditTrain(gridView, TrainDirection.tr, false);

            UpdateListView(gridView, TrainDirection.tr);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;
            else
                gridView.KeyDown += HandleKeystroke;
            
            gridView.SelectedItemsChanged += GridViewOnSelectedItemsChanged;

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }

        private void GridViewOnSelectedItemsChanged(object sender, EventArgs e)
        {
            editButton.Enabled = deleteButton.Enabled = copyButton.Enabled = editPathButton.Enabled
                = gridView.SelectedItem != null && !((ITrain) gridView.SelectedItem).IsLink;
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
                DeleteTrain(gridView, TrainDirection.tr, false);
            else if ((e.Key == Keys.C && e.Control))
                CopyTrain(gridView, false);
            else if ((e.Key == Keys.P && e.Control))
                EditPath(gridView, false);
            else if ((e.Key == Keys.B && e.Control) || (e.Key == Keys.Enter))
                EditTrain(gridView, TrainDirection.tr, false);
            else if (e.Key == Keys.N && e.Control)
                NewTrain(gridView);
        }

        private string BuildPath(ITrain t)
        {
            var path = t.GetPath();
            return path.FirstOrDefault()?.SName + " - " + path.LastOrDefault()?.SName;
        }

        private void EditPath(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                if (view.SelectedItem is Train train)
                {
                    using (var tpf = TrainPathForm.EditPath(pluginInterface, train))
                        if (tpf.ShowModal(this) == DialogResult.Ok)
                            UpdateListView(view, TrainDirection.tr);
                }
                else if (message)
                        MessageBox.Show("Verlinke Züge können nicht bearbeitet werden.", "Zug bearbeiten");
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Laufweg bearbeiten");
        }

        private void CopyTrain(GridView view, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                if (view.SelectedItem is Train train)
                {
                    using (var tcf = new TrainCopyDialog(train, pluginInterface.Timetable))
                        tcf.ShowModal(this);

                    UpdateListView(view, TrainDirection.tr);
                }
                else if (message)
                    MessageBox.Show("Verlinke Züge können nicht kopiert werden.", "Zug bearbeiten");
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }

        private void NewTrain(GridView view)
        {
            using (var tpf = TrainPathForm.NewTrain(pluginInterface))
            {
                if (tpf.ShowModal(this) != DialogResult.Ok)
                    return;

                using (var tef = new TrainEditForm(pluginInterface.Timetable, TrainDirection.tr, tpf.Path))
                {
                    if (tef.ShowModal(this) == DialogResult.Ok)
                    {
                        tt.AddTrain(tef.Train);
                        if (tef.NextTrain != null)
                            tt.SetTransition(tef.Train, tef.NextTrain);

                        UpdateListView(view, TrainDirection.tr);
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            pluginInterface.ClearBackup(backupHandle);
            Result = DialogResult.Ok;
            this.NClose();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            pluginInterface.RestoreTimetable(backupHandle);
            this.NClose();
        }

        private void NewButton_Click(object sender, EventArgs e)
            => NewTrain(gridView);

        private void EditButton_Click(object sender, EventArgs e)
            => EditTrain(gridView, TrainDirection.tr);

        private void DeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(gridView, TrainDirection.tr);

        private void CopyButton_Click(object sender, EventArgs e)
            => CopyTrain(gridView);

        private void EditPathButton_Click(object sender, EventArgs e)
            => EditPath(gridView);

        private void SortButton_Click(object sender, EventArgs e)
            => SortTrains(gridView, TrainDirection.tr);
    }
}
