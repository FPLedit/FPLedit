﻿using Eto.Forms;
using FPLedit.Editor.Trains;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.Editor.Linear
{
    internal sealed class LinearTrainsEditForm : BaseTrainsEditor
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly object backupHandle;

#pragma warning disable CS0649
        private readonly GridView topGridView, bottomGridView;
        private readonly Label topLineLabel, bottomLineLabel;
        private readonly Button topEditButton, topDeleteButton, topCopyButton, bottomEditButton, bottomDeleteButton, bottomCopyButton;
#pragma warning restore CS0649

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        private GridView active;

        public LinearTrainsEditForm(IPluginInterface pluginInterface) : base(pluginInterface.Timetable)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            this.pluginInterface = pluginInterface;
            tt = pluginInterface.Timetable;
            backupHandle = pluginInterface.BackupTimetable();

            InitListView(topGridView, new []{ topEditButton, topDeleteButton, topCopyButton });
            InitListView(bottomGridView, new []{ bottomEditButton, bottomDeleteButton, bottomCopyButton });

            topLineLabel.Text = "Züge " + tt.GetLinearLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + tt.GetLinearLineName(BOTTOM_DIRECTION);
            UpdateListView(topGridView, TOP_DIRECTION);
            UpdateListView(bottomGridView, BOTTOM_DIRECTION);

            bottomGridView.MouseDoubleClick += (s, e) => EditTrain(bottomGridView, BOTTOM_DIRECTION, false);
            topGridView.MouseDoubleClick += (s, e) => EditTrain(topGridView, TOP_DIRECTION, false);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            TrainDirection dir;
            if (active == topGridView)
                dir = TOP_DIRECTION;
            else
                dir = BOTTOM_DIRECTION;

            if (active == null)
                return;

            if (e.Key == Keys.Delete)
                DeleteTrain(active, dir, false);
            else if ((e.Key == Keys.B && e.Control) || (e.Key == Keys.Enter))
                EditTrain(active, dir, false);
            else if (e.Key == Keys.N && e.Control)
                NewTrain(active, dir);
            else if (e.Key == Keys.C && e.Control)
                CopyTrain(active, dir);
        }

        private void InitListView(GridView view, Button[] buttons)
        {
            view.AddColumn<ITrain>(t => t.TName, "Zugnummer");
            view.AddColumn<ITrain>(t => t.Locomotive, "Tfz");
            view.AddColumn<ITrain>(t => t.Mbr, "Mbr");
            view.AddColumn<ITrain>(t => t.Last, "Last");
            view.AddColumn<ITrain>(t => t.Days.DaysToString(false), "Verkehrstage");
            view.AddColumn<ITrain>(t => t.Comment, "Kommentar");

            view.GotFocus += (s, e) => active = view;

            if (!Eto.Platform.Instance.IsWpf)
                view.KeyDown += HandleKeystroke;

            view.SelectedItemsChanged += (s, e) =>
            {
                foreach (var button in buttons)
                    button.Enabled = view.SelectedItem != null && !((ITrain) view.SelectedItem).IsLink;
            };
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

        #region Events
        private void TopNewButton_Click(object sender, EventArgs e)
            => NewTrain(topGridView, TOP_DIRECTION);

        private void TopEditButton_Click(object sender, EventArgs e)
            => EditTrain(topGridView, TOP_DIRECTION);

        private void TopDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(topGridView, TOP_DIRECTION);

        private void TopSortButton_Click(object sender, EventArgs e)
            => SortTrains(topGridView, TOP_DIRECTION);

        private void BottomNewButton_Click(object sender, EventArgs e)
            => NewTrain(bottomGridView, BOTTOM_DIRECTION);

        private void BottomEditButton_Click(object sender, EventArgs e)
            => EditTrain(bottomGridView, BOTTOM_DIRECTION);

        private void BottomDeleteButton_Click(object sender, EventArgs e)
            => DeleteTrain(bottomGridView, BOTTOM_DIRECTION);

        private void TopCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(topGridView, TOP_DIRECTION, true);

        private void BottomCopyButton_Click(object sender, EventArgs e)
            => CopyTrain(bottomGridView, BOTTOM_DIRECTION, true);

        private void BottomSortButton_Click(object sender, EventArgs e)
            => SortTrains(bottomGridView, BOTTOM_DIRECTION);
        #endregion
    }
}
