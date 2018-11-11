using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Linear
{
    internal class TrainsEditForm : TrainsEditorBase
    {
        private IInfo info;
        private Timetable tt;

#pragma warning disable CS0649
        private GridView topGridView, bottomGridView;
        private Label topLineLabel, bottomLineLabel;
#pragma warning restore CS0649

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        private GridView active;

        public TrainsEditForm(IInfo info) : base(info.Timetable)
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

            bottomGridView.MouseDoubleClick += (s, e) => EditTrain(bottomGridView, BOTTOM_DIRECTION, false);
            topGridView.MouseDoubleClick += (s, e) => EditTrain(topGridView, TOP_DIRECTION, false);

            if (Eto.Platform.Instance.IsWpf)
                KeyDown += HandleKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }

        private void HandleKeystroke(object sender, KeyEventArgs e)
        {
            TrainDirection dir = default(TrainDirection);

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
        }

        private void InitListView(GridView view)
        {
            view.AddColumn<Train>(t => t.TName, "Zugnummer");
            view.AddColumn<Train>(t => t.Locomotive, "Tfz");
            view.AddColumn<Train>(t => t.Mbr, "Mbr");
            view.AddColumn<Train>(t => t.Last, "Last");
            view.AddColumn<Train>(t => DaysHelper.DaysToString(t.Days, false), "Verkehrstage");
            view.AddColumn<Train>(t => t.Comment, "Kommentar");

            view.GotFocus += (s, e) => active = view;

            if (!Eto.Platform.Instance.IsWpf)
                view.KeyDown += HandleKeystroke;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            info.ClearBackup();
            Result = DialogResult.Ok;
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();
            this.NClose();
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
