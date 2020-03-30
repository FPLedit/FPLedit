using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;
using System.Linq.Expressions;
using Eto.DsBindComboBoxCell;

namespace FPLedit.Editor.TimetableEditor
{
    internal sealed class SingleTimetableEditControl : BaseTimetableEditControl
    {
#pragma warning disable CS0649
        private readonly GridView dataGridView;
        private readonly ToggleButton trapeztafelToggle;
        private readonly Button zlmButton, shuntButton;
        private readonly TableLayout actionsLayout;
#pragma warning restore CS0649

        private Timetable tt;
        private Train train;
        private List<Station> path;

        private const string NO_TRACK = "<Kein Gleis>";

        protected override int FirstEditingColumn => 2; // erstes Abfahrtsfeld (Ankunft ja deaktiviert)

        public SingleTimetableEditControl() : base()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            trapeztafelToggle.Click += TrapeztafelToggle_Click;
            base.Init(trapeztafelToggle, actionsLayout);

            KeyDown += HandleControlKeystroke;

            trapeztafelToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png"));
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

        private CustomCell GetCell(Func<ArrDep, TimeEntry> time, bool arrival)
        {
            var cc = new CustomCell
            {
                CreateCell = args => new TextBox(),
                ConfigureCell = (args, control) =>
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
                }
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

        private DsBindComboBoxCell GetTrackCell(Expression<Func<ArrDep, string>> track)
        {
            var cc = new DsBindComboBoxCell();

            var shadowBinding = Binding.Property(track);

            cc.Binding = Binding.Delegate<DataElement, string>(
                d => shadowBinding.GetValue(d.ArrDeps[d.Station]),
                (d, t) => shadowBinding.SetValue(d.ArrDeps[d.Station], t == (string)d.GetTrackDataStore().FirstOrDefault() ? "" : t))
                .Cast<object>();
            cc.DataStoreBinding = Binding.Delegate<DataElement, IEnumerable<object>>(d => d.GetTrackDataStore());
            return cc;
        }
        
        private CheckBoxCell GetCheckCell(Expression<Func<ArrDep, bool>> check)
        {
            var cc = new CheckBoxCell();
            
            var shadowBinding = Binding.Property(check);
            
            cc.Binding = Binding.Delegate<DataElement, bool>(d => shadowBinding.GetValue(d.ArrDeps[d.Station])).Cast<bool?>();
            return cc;
        }

        protected override void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;

            trapeztafelToggle.Enabled = arrival && !data.IsFirst(sta);
            zlmButton.Enabled = arrival ^ data.IsFirst(sta);
        }

        private class DataElement : BaseTimetableDataElement
        {
            public Station Station { get; set; }

            public override Station GetStation() => Station;

            public IEnumerable<object> GetTrackDataStore()
            {
                var ds = Station.Tracks.Select(s => s.Name).ToList();
                ds.Insert(0, NO_TRACK);
                return ds;
            }

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

            if (tt.Version.Compare(TimetableVersion.JTG3_1) >= 0)
            {
                view.AddColumn(GetTrackCell(t => t.ArrivalTrack), "Ankunftsgleis", editable: true);
                view.AddColumn(GetTrackCell(t => t.DepartureTrack), "Abfahrtsgleis", editable: true);
                view.AddColumn(GetCheckCell(t => t.ShuntMoves.Any()), "Rangiert", editable: false);
            }
            else
                shuntButton.Visible = false;

            view.KeyDown += (s, e) => HandleViewKeystroke(e, view);

            view.DataStore = path.Select(sta => new DataElement(train, sta, train.GetArrDep(sta))).ToList();
            view.SelectedRowsChanged += (s, e) =>
            {
                var selected = (DataElement)view.SelectedItem;
                shuntButton.Enabled = selected?.Station.Tracks.Any() ?? false;
            };
        }

        protected override Point GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e)
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
            }
            return true;
        }

        #region Events
        public bool ApplyChanges()
            => UpdateTrainDataFromGrid(dataGridView);

        private void TrapeztafelToggle_Click(object sender, EventArgs e)
            => Trapez(dataGridView);

        private void ZlmButton_Click(object sender, EventArgs e)
            => Zuglaufmeldung(dataGridView);

        private void ShuntButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRow == -1)
                return;

            var data = (DataElement)dataGridView.SelectedItem;
            var sta = data.Station;

            var arrDep = data.ArrDeps[sta];

            using (var shf = new ShuntForm(arrDep, sta))
                if (shf.ShowModal(this) != DialogResult.Ok)
                    return;

            dataGridView.ReloadData(dataGridView.SelectedRow);
        }
        #endregion
    }
}
