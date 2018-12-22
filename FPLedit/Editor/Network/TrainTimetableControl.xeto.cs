using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;
using Eto;
using System.Collections.ObjectModel;

namespace FPLedit.Editor.Network
{
    internal class TrainTimetableControl : TimetableEditorBase
    {
#pragma warning disable CS0649
        private GridView dataGridView;
        private Button internalToggle;
        private ToggleButton trapeztafelToggle;
        private Button zlmButton;
#pragma warning restore CS0649

        private Timetable tt;
        private Train train;
        private List<Station> path;

        protected override int FirstEditingColumn => 2; // erstes Abfahrtsfeld (Ankunft ja deaktiviert)

        private ObservableCollection<Control> actionButtons = new ObservableCollection<Control>();
        public IList<Control> ActionButtons => actionButtons;

        public TrainTimetableControl()
        {
            actionButtons.CollectionChanged += (s, e) =>
            {
                var row = FindChild<TableLayout>("actionsLayout").Rows[0];
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
                Trapez(dataGridView);
            }
            else if (e.Key == Keys.Z)
            {
                e.Handled = true;
                Zuglaufmeldung(dataGridView);
            }
        }

        public void Initialize(Timetable tt, Train t)
        {
            this.tt = tt;

            train = t;
            path = t.GetPath();

            InitializeGridView(dataGridView);
            Initialized = true;
        }

        private CustomCell GetCell(Func<ArrDep, TimeSpan> time, bool arrival)
        {
            var cc = new CustomCell();
            cc.CreateCell = args => new TextBox();
            cc.ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                var data = (DataElement)args.Item;
                new TimetableCellRenderProperties(time, data.Station, arrival, data).Apply(tb);

                if (mpmode)
                {
                    tb.KeyDown += (s, e) => HandleKeystroke(e, dataGridView);

                    // Wir gehen hier gleich in den vollen EditMode rein
                    tb.CaretIndex = 0;
                    tb.SelectAll();
                    CellSelected(data, data.Station, arrival); data.IsSelectedArrival = arrival; data.SelectedTextBox = tb;
                }

                tb.GotFocus += (s, e) => { CellSelected(data, data.Station, arrival); data.IsSelectedArrival = arrival; data.SelectedTextBox = tb; };
                tb.LostFocus += (s, e) => { FormatCell(data, data.Station, arrival, tb); new TimetableCellRenderProperties(time, data.Station, arrival, data).Apply(tb); };
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode)
                    return;

                var data = (DataElement)e.Item;
                new TimetableCellRenderProperties(time, data.Station, arrival, data).Render(e.Graphics, e.ClipRectangle);
            };
            return cc;
        }

        protected override void CellSelected(TimetableDataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;

            internalToggle.Enabled = arrival && !data.IsFirst(sta);
            zlmButton.Enabled = arrival ^ data.IsFirst(sta);
        }

        private class DataElement : TimetableDataElement
        {
            public Station Station { get; set; }

            public override Station GetStation() => Station;

            public DataElement(Train tra, Station sta, ArrDep arrDep)
            {
                Train = tra;
                ArrDeps = new Dictionary<Station, ArrDep>(1) { [sta] = arrDep };
                Station = sta;
            }
        }

        private void InitializeGridView(GridView view)
        {
            view.Columns.Clear();
            view.AddColumn<DataElement>(t => t.Station.SName, "Bahnhof");
            view.AddColumn(GetCell(t => t.Arrival, true), "Ankunft");
            view.AddColumn(GetCell(t => t.Departure, false), "Abfahrt");

            view.KeyDown += (s, e) => HandleViewKeystroke(e, view);

            view.DataStore = path.Select(sta => new DataElement(train, sta, train.GetArrDep(sta))).ToList();
        }

        protected override Point GetNextEditingPosition(TimetableDataElement data, GridView view, KeyEventArgs e)
        {
            var arrival = data.IsSelectedArrival;
            var idx = arrival ? 2 : 1;
            int row;
            if (!e.Control)
                row = view.SelectedRow + (arrival ? 0 : 1);
            else
                row = view.SelectedRow - (arrival ? 1 : 0);
            return new Point(row, idx);
        }

        private bool UpdateTrainDataFromGrid(GridView view)
        {
            foreach (DataElement row in view.DataStore)
            {
                if (row.HasAnyError)
                {
                    MessageBox.Show("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
                    return false;
                }

                train.SetArrDep(row.Station, row.ArrDeps[row.Station]);
            }
            return true;
        }

        #region Events
        public bool ApplyChanges()
            => UpdateTrainDataFromGrid(dataGridView);

        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => Trapez(dataGridView);

        private void zlmButton_Click(object sender, EventArgs e)
            => Zuglaufmeldung(dataGridView);
        #endregion
    }
}
