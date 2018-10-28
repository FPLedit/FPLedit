using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Linear
{
    internal class TimetableEditForm : TimetableEditorBase
    {
        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

#pragma warning disable CS0649
        private GridView topDataGridView, bottomDataGridView;
        private Label topLineLabel, bottomLineLabel;
        private Button internalToggle;
        private ToggleButton trapeztafelToggle;
        private Button zlmButton;
#pragma warning restore CS0649

        private GridView focused;

        private IInfo info;

        protected override int FirstEditingColumn => 1; // erstes Abfahrtsfeld

        public TimetableEditForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            trapeztafelToggle = new ToggleButton(internalToggle);
            trapeztafelToggle.ToggleClick += trapeztafelToggle_Click;
            base.Init(trapeztafelToggle);

            this.info = info;
            info.BackupTimetable();

            topLineLabel.Text = "Züge " + info.Timetable.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + info.Timetable.GetLineName(BOTTOM_DIRECTION);

            InitializeGridView(topDataGridView, TOP_DIRECTION);
            InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);

            KeyDown += (s, e) =>
            {
                if (e.Key == Keys.T)
                {
                    e.Handled = true;
                    ViewDependantAction(Trapez);
                }
                else if (e.Key == Keys.Z)
                {
                    e.Handled = true;
                    ViewDependantAction(Zuglaufmeldung);
                }
            };

            internalToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png"));

            if (mpmode)
                DefaultButton = null; // Bugfix, Window closes on enter [Enter]

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }

        private CustomCell GetCell(Func<ArrDep, TimeSpan> time, Station sta, bool arrival, GridView view)
        {
            var cc = new CustomCell();
            cc.CreateCell = args => new TextBox();
            cc.ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                var data = (DataElement)args.Item;

                if (data == null)
                {
                    tb.Visible = false;
                    return;
                }

                new TimetableCellRenderProperties(time, sta, arrival, data).Apply(tb);

                Action tbEnterEditMode = new Action(() =>
                {
                    CellSelected(data, sta, arrival);
                    data.IsSelectedArrival = arrival;
                    data.SelectedStation = sta;
                    data.SelectedTextBox = tb;
                    focused = view;
                });

                if (mpmode)
                {
                    tb.KeyDown += (s, e) => HandleKeystroke(e, focused);

                    // Wir gehen hier gleich in den vollen EditMode rein
                    tb.CaretIndex = 0;
                    tb.SelectAll();
                    tbEnterEditMode();
                }

                tb.GotFocus += (s, e) => tbEnterEditMode();
                tb.LostFocus += (s, e) => { FormatCell(data, sta, arrival, tb); new TimetableCellRenderProperties(time, sta, arrival, data).Apply(tb); };
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode)
                    return;

                var data = (DataElement)e.Item;
                if (data == null)
                    return;

                new TimetableCellRenderProperties(time, sta, arrival, data).Render(e.Graphics, e.ClipRectangle);
            };
            return cc;
        }

        protected override Point GetNextEditingPosition(TimetableDataElement data, GridView view, KeyEventArgs e)
        {
            var path = data.Train.GetPath();
            int idx, row;
            if (!e.Control)
            {
                idx = path.IndexOf(data.GetStation()) * 2 + (data.IsSelectedArrival ? 1 : 2);
                row = view.SelectedRow;
                if (path.Last() == data.GetStation() && data.IsSelectedArrival)
                {
                    idx = 1;
                    row++;
                }
            }
            else
            {
                idx = path.IndexOf(data.GetStation()) * 2 - (data.IsSelectedArrival ? 1 : 0);
                row = view.SelectedRow;
                if (path.First() == data.GetStation() && !data.IsSelectedArrival)
                {
                    idx = (path.Count - 1) * 2;
                    row--;
                }
            }
            return new Point(row, idx);
        }

        protected override void CellSelected(TimetableDataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;

            internalToggle.Enabled = arrival;
            zlmButton.Enabled = arrival ^ (data.IsFirst(sta));
        }

        private class DataElement : TimetableDataElement
        {
            public Station SelectedStation { get; set; }

            public override Station GetStation() => SelectedStation;
        }

        private void InitializeGridView(GridView view, TrainDirection dir)
        {
            var stations = info.Timetable.GetStationsOrderedByDirection(dir);

            view.AddColumn<DataElement>(t => t.Train.TName, "Zugnummer");
            foreach (var sta in stations)
            {
                if (sta != stations.First())
                    view.AddColumn(GetCell(t => t.Arrival, sta, true, view), sta.SName + " an");
                if (sta != stations.Last())
                    view.AddColumn(GetCell(t => t.Departure, sta, false, view), sta.SName + " ab");
            }

            var l = info.Timetable.Trains.Where(t => t.Direction == dir).Select(tra => new DataElement()
            {
                Train = tra,
                ArrDeps = tra.GetArrDeps()
            }).ToList();

            view.GotFocus += (s, e) => focused = view;
            view.KeyDown += (s, e) => HandleViewKeystroke(e, view);
            if (mpmode)
                l.Add(null);

            view.DataStore = l;
        }

        private bool UpdateTrainDataFromGrid(Train train, GridView view)
        {
            foreach (DataElement row in view.DataStore)
            {
                if (row.Train != train)
                    continue;

                if (row.HasAnyError)
                {
                    MessageBox.Show("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
                    return false;
                }

                foreach (var sta in row.Train.GetPath())
                    train.SetArrDep(sta, row.ArrDeps[sta]);

                return true;
            }

            throw new Exception("Zug, der am Anfang noch da war, ist verschwunden!");
        }

        private void ViewDependantAction(Action<GridView> action)
        {
            if (focused == topDataGridView)
                action(topDataGridView);
            else if (focused == bottomDataGridView)
                action(bottomDataGridView);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            foreach (var t in info.Timetable.Trains)
            {
                var ret = true;
                if (t.Direction == TOP_DIRECTION)
                    ret &= UpdateTrainDataFromGrid(t, topDataGridView);
                else
                    ret &= UpdateTrainDataFromGrid(t, bottomDataGridView);

                if (!ret)
                    return;
            }

            info.ClearBackup();
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();

            this.NClose();
        }

        #region Events
        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => ViewDependantAction(Trapez);

        private void zlmButton_Click(object sender, EventArgs e)
            => ViewDependantAction(Zuglaufmeldung);
        #endregion
    }
}
