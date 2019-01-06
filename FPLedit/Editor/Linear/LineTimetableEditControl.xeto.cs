using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FPLedit.Editor.Linear
{
    internal class LineTimetableEditControl : BaseTimetableEditControl
    {
        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

#pragma warning disable CS0649
        private GridView topDataGridView, bottomDataGridView;
        private Label topLineLabel, bottomLineLabel;
        private Button internalToggle;
        private ToggleButton trapeztafelToggle;
        private Button zlmButton;
        private TableLayout actionsLayout;
#pragma warning restore CS0649

        private GridView focused;

        private Timetable tt;

        protected override int FirstEditingColumn => 1; // erstes Abfahrtsfeld

        private ObservableCollection<Control> actionButtons;
        public IList<Control> ActionButtons => actionButtons;

        public LineTimetableEditControl()
        {
            actionButtons = new ObservableCollection<Control>();
            actionButtons.CollectionChanged += (s, e) =>
            {
                var row = actionsLayout.Rows[0];
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    foreach (Control btn in e.NewItems)
                        row.Cells.Add(btn);
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (Control btn in e.OldItems)
                        row.Cells.Remove(btn);
            };

            Eto.Serialization.Xaml.XamlReader.Load(this);
            trapeztafelToggle = new ToggleButton(internalToggle);
            trapeztafelToggle.ToggleClick += trapeztafelToggle_Click;
            base.Init(trapeztafelToggle);

            KeyDown += HandleControlKeystroke;

            internalToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png"));
        }

        public void HandleControlKeystroke(object sender, KeyEventArgs e)
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
        }

        public void Initialize(Timetable tt)
        {
            this.tt = tt;

            topLineLabel.Text = "Züge " + this.tt.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + this.tt.GetLineName(BOTTOM_DIRECTION);

            InitializeGridView(topDataGridView, TOP_DIRECTION);
            InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);

            Initialized = true;
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

        protected override Point GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e)
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

        protected override void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;

            internalToggle.Enabled = arrival;
            zlmButton.Enabled = arrival ^ (data.IsFirst(sta));
        }

        private class DataElement : BaseTimetableDataElement
        {
            public Station SelectedStation { get; set; }

            public override Station GetStation() => SelectedStation;
        }

        private string GetTransition(Train t) => tt.GetTransition(t)?.TName ?? "";

        private void InitializeGridView(GridView view, TrainDirection dir)
        {
            var stations = tt.GetStationsOrderedByDirection(dir);

            view.AddColumn<DataElement>(t => t.Train.TName, "Zugnummer");
            foreach (var sta in stations)
            {
                if (sta != stations.First())
                    view.AddColumn(GetCell(t => t.Arrival, sta, true, view), sta.SName + " an");
                if (sta != stations.Last())
                    view.AddColumn(GetCell(t => t.Departure, sta, false, view), sta.SName + " ab");
            }
            view.AddColumn<DataElement>(t => GetTransition(t.Train), "Folgezug");

            var l = tt.Trains.Where(t => t.Direction == dir).Select(tra => new DataElement()
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

        public bool ApplyChanges()
        {
            foreach (var t in tt.Trains)
            {
                var ret = true;
                if (t.Direction == TOP_DIRECTION)
                    ret &= UpdateTrainDataFromGrid(t, topDataGridView);
                else
                    ret &= UpdateTrainDataFromGrid(t, bottomDataGridView);

                if (!ret)
                    return false;
            }
            return true;
        }

        #region Events
        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => ViewDependantAction(Trapez);

        private void zlmButton_Click(object sender, EventArgs e)
            => ViewDependantAction(Zuglaufmeldung);
        #endregion
    }
}
