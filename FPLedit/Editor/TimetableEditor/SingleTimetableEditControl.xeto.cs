using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.TimetableEditor
{
    internal sealed class SingleTimetableEditControl : BaseTimetableEditControl
    {
#pragma warning disable CS0649
        private readonly GridView dataGridView;
        private readonly ToggleButton trapeztafelToggle, requestToggle;
        private readonly Button zlmButton, shuntButton;
        private readonly TableLayout actionsLayout;
#pragma warning restore CS0649

        private Timetable tt;
        private IWritableTrain train;
        private List<Station> path;

        private const string NO_TRACK = "<Kein Gleis>";

        protected override int FirstEditingColumn => 2; // erstes Abfahrtsfeld (Ankunft ja deaktiviert)

        public SingleTimetableEditControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            trapeztafelToggle.Click += TrapeztafelToggle_Click;
            requestToggle.Click += RequestToggleOnClick;
            Init(trapeztafelToggle, actionsLayout);

            KeyDown += HandleControlKeystroke;
            dataGridView.KeyDown += HandleControlKeystroke;
            
#pragma warning disable CA2000
            trapeztafelToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png")).WithSize(50, 30);
            requestToggle.Image = new Bitmap(this.GetResource("Resources.bedarf.png")).WithSize(50, 30);
#pragma warning disable CA2000
        }

        public void HandleControlKeystroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.T)
            {
                e.Handled = true;
                Trapez(dataGridView);
            }
            if (e.Key == Keys.B)
            {
                e.Handled = true;
                RequestStop(dataGridView);
            }
            else if (e.Key == Keys.Z)
            {
                e.Handled = true;
                Zuglaufmeldung(dataGridView);
            }
            else if (e.Key == Keys.R)
            {
                e.Handled = true;
                Shunt(dataGridView);
            }
            else
                HandleKeystroke(e, dataGridView);
        }

        public void Initialize(Timetable timetable, IWritableTrain t)
        {
            tt = timetable;

            train = t;
            path = t.GetPath();

            InitializeGridView(dataGridView);
            Initialized = true;
        }

        private CustomCell GetCell(Func<ArrDep, TimeEntry> time, bool arrival)
        {
            var cc = new CustomCell
            {
                CreateCell = args =>
                {
                    var tb = new TextBox { Tag = new CCCO() };

                    if (mpmode)
                        tb.KeyDown += (s, e) => HandleKeystroke(e, dataGridView);
                    
                    tb.GotFocus += (s, e) =>
                    {
                        var dd2 = (TextBox)s;
                        var ccco = (CCCO)dd2!.Tag;
                        if (ccco.InhibitEvents)
                            return;
                        CellSelected(ccco.Data, ccco.Data.GetStation(), arrival);
                        ccco.Data.IsSelectedArrival = arrival;
                        ccco.Data.SelectedTextBox = tb;
                    };
                    
                    tb.LostFocus += (s, e) =>
                    {
                        var dd2 = (TextBox)s;
                        var ccco = (CCCO)dd2!.Tag;
                        if (ccco.InhibitEvents)
                            return;
                        FormatCell(ccco.Data, ccco.Data.GetStation(), arrival, tb);
                        new TimetableCellRenderProperties(time, ccco.Data.GetStation(), arrival, ccco.Data).Apply(tb);
                    };
                    
                    return tb;
                },
                ConfigureCell = (args, control) =>
                {
                    var tb = (TextBox) control;
                    if (tb == null)
                        return;
                    var ccco = (CCCO) tb.Tag;
                    ccco.InhibitEvents = true;

                    if (args?.Item == null)
                        return;
                    
                    var data = (DataElement) args.Item;
                    ccco.Data = data;
                    new TimetableCellRenderProperties(time, data.Station, arrival, data).Apply(tb);

                    ccco.InhibitEvents = false;

                    if (mpmode)
                    {
                        // Wir gehen hier gleich in den vollen EditMode rein
                        tb.CaretIndex = 0;
                        tb.SelectAll();
                        CellSelected(data, data.Station, arrival);
                        data.IsSelectedArrival = arrival;
                        data.SelectedTextBox = tb;
                    }

                    /*tb.GotFocus += (s, e) =>
                    {
                        CellSelected(data, data.Station, arrival);
                        data.IsSelectedArrival = arrival;
                        data.SelectedTextBox = tb;
                    };*/
                    
                    //if (Platform.IsMac)
                    //    tb.KeyUp += (s, e) => data.SetTmpTag(data.Station, arrival, tb.Text);
                    /*tb.LostFocus += (s, e) =>
                    {
                        FormatCell(data, data.Station, arrival, tb);
                        new TimetableCellRenderProperties(time, data.Station, arrival, data).Apply(tb);
                    };;*/
                }
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode) return;
                var data = (DataElement) e.Item;
                new TimetableCellRenderProperties(time, data.Station, arrival, data).Render(e.Graphics, e.ClipRectangle);
            };
            return cc;
        }

        private CustomCell GetTrackCell(Func<ArrDep, string> track, Action<ArrDep, string> setTrack, bool arrival)
        {
            var cc = new CustomCell
            {
                CreateCell = args =>
                {
                    var dd = new DropDown { Tag = new CCCO() };

                    dd.SelectedIndexChanged += (s, e) =>
                    {
                        var dd2 = (DropDown)s;
                        var ccco = (CCCO)dd2!.Tag;
                        if (ccco.InhibitEvents)
                            return;
                        var t = (string) dd.SelectedValue;
                        var ds = ((DataElement)ccco.Data).GetTrackDataStore().ToArray();
                        if (dd.SelectedIndex > -1)
                            Console.WriteLine("SelectedValue: " + t + "/" + ds[dd.SelectedIndex]);
                        else
                            Console.WriteLine("Selected index: " + dd.SelectedIndex);
                        
                        
                        setTrack(ccco.Data.ArrDeps[ccco.Data.GetStation()], t == (string) dd.DataStore.FirstOrDefault() ? "" : t);
                    };
                    
                    dd.GotFocus += (s, e) =>
                    {
                        var dd2 = (DropDown)s;
                        var ccco = (CCCO)dd2!.Tag;
                        if (ccco.InhibitEvents)
                            return;
                        ccco.Data.IsSelectedArrival = arrival;
                        ccco.Data.SelectedTextBox = null;
                        ccco.Data.SelectedDropDown = dd;
                    };
                    
                    dd.KeyDown += (s, e) => HandleKeystroke(e, dataGridView);
                    
                    return dd;
                },
                ConfigureCell = (args, control) =>
                {
                    var dd = (DropDown) control;
                    if (dd == null)
                        return;
                    var ccco = (CCCO) dd.Tag;
                    ccco.InhibitEvents = true; // Inihibit event handling while new state is set.

                    if (args?.Item == null)
                        return;
                    
                    var data = (DataElement) args.Item;
                    ccco.Data = data;

                    var ds = data.GetTrackDataStore().ToArray();

                    dd.DataStore = ds;
                    dd.Enabled = ds.Length > 1;
                    if (data.IsFirst(data.Station) && arrival || data.IsLast(data.Station) && !arrival)
                        dd.Enabled = false;
                    
                    dd.SelectedValue = track(data.ArrDeps[data.Station]);
                    
                    ccco.InhibitEvents = false; // Resume event handling of cutom control.

                    if (mpmode)
                    {
                        data.IsSelectedArrival = arrival;
                        data.SelectedTextBox = null;
                        data.SelectedDropDown = dd;
                        dd.Focus();
                    }
                },
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode) return;
                var data = (DataElement) e.Item;
                new TimetableCellRenderProperties2(track(data.ArrDeps[data.Station])).Render(e.Graphics, e.ClipRectangle);
            };
            return cc;
        }

        private CheckBoxCell GetCheckCell(Func<ArrDep, bool> check)
        {
            return new CheckBoxCell
            {
                Binding = Binding.Delegate<DataElement, bool>(d => check(d.ArrDeps[d.Station])).Cast<bool?>()
            };
        }

        protected override void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;
            requestToggle.Checked = data.ArrDeps[sta].RequestStop || sta.RequestStop;

            trapeztafelToggle.Enabled = requestToggle.Enabled = arrival && !data.IsFirst(sta) && !sta.RequestStop;
            zlmButton.Enabled = arrival ^ data.IsFirst(sta);
        }

        private class DataElement : BaseTimetableDataElement
        {
            public Station Station { get; }

            public override Station GetStation() => Station;

            public IEnumerable<object> GetTrackDataStore()
            {
                var ds = Station.Tracks.Select(s => s.Name).ToList();
                ds.Insert(0, NO_TRACK);
                return ds;
            }
            
            public void SetRequestStop(Station sta, bool req)
            {
                if (sta.RequestStop)
                    return;
                var a = ArrDeps[sta];
                a.RequestStop = req;
                ArrDeps[sta] = a;
            }

            public DataElement(ITrain tra, Station sta, ArrDep arrDep)
            {
                Train = tra;
                ArrDeps = new Dictionary<Station, ArrDep>(1) { [sta] = arrDep };
                Station = sta;
            }
        }

        private void InitializeGridView(GridView view)
        {
#pragma warning disable CA2000
            view.Columns.Clear();
            view.AddColumn<DataElement>(t => t.Station.SName, T._("Bahnhof"));
            view.AddColumn(GetCell(t => t.Arrival, true), T._("Ankunft"));
            view.AddColumn(GetCell(t => t.Departure, false), T._("Abfahrt"));
            view.AddColumn(GetTrackCell(t => t.ArrivalTrack, (t,s) => t.ArrivalTrack = s, true), T._("Ankunftsgleis"), editable: true);
            view.AddColumn(GetTrackCell(t => t.DepartureTrack, (t,s) => t.ArrivalTrack = s, false), T._("Abfahrtsgleis"), editable: true);
            view.AddColumn(GetCheckCell(t => t.ShuntMoves.Any()), T._("Rangiert"), editable: false);
#pragma warning restore CA2000

            view.KeyDown += (s, e) => HandleViewKeystroke(e, view);

            view.DataStore = path.Select(sta => new DataElement(train, sta, train.GetArrDep(sta))).ToList();
            view.SelectedRowsChanged += (s, e) =>
            {
                var selected = (DataElement) view.SelectedItem;
                shuntButton.Enabled = selected?.Station.Tracks.Any() ?? false;
            };
        }

        protected override Point GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e)
        {
            var arrival = data.IsSelectedArrival;
            var isTime = data.SelectedTextBox == null;
            int idx = (arrival ? 1 : 2) + (isTime ? 2 : 0);
            int row = view.SelectedRow;
            if (!e.Control)
            {
                idx++;
                if (idx > 4)
                {
                    idx = 1;
                    row++;
                }
            }
            else
            {
                idx--;
                if (idx < 1)
                {
                    idx = 4;
                    row--;
                }
            }

            return new Point(row, idx);
        }

        private bool UpdateTrainDataFromGrid(GridView view)
        {
            foreach (DataElement row in view.DataStore)
            {
                if (row.HasAnyError)
                {
                    MessageBox.Show(T._("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!"));
                    return false;
                }
            }

            return true;
        }

        private void Shunt(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement) view.SelectedItem;
            var sta = data.Station;

            if (!sta.Tracks.Any()) // We have no tracks.
                return;

            var arrDep = data.ArrDeps[sta];

            using (var shf = new ShuntForm(train, arrDep, sta))
                if (shf.ShowModal(this) != DialogResult.Ok)
                    return;

            view.ReloadData(view.SelectedRow);
        }
        
        private void RequestStop(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement)view.SelectedItem;

            // Request Stop darf nur bei Ankünften sein
            if (!data.IsSelectedArrival)
                return;

            var sta = data.GetStation();

            // All traisn stop at this station.
            if (sta.RequestStop)
                return;
            
            var req = !data.ArrDeps[sta].RequestStop;
            data.SetRequestStop(sta, req);

            view.ReloadData(view.SelectedRow);
            requestToggle.Checked = req;
            CellSelected(data, sta, data.IsSelectedArrival);
        }

        #region Events

        public bool ApplyChanges() => UpdateTrainDataFromGrid(dataGridView);
        private void TrapeztafelToggle_Click(object sender, EventArgs e) => Trapez(dataGridView);
        private void RequestToggleOnClick(object sender, EventArgs e) => RequestStop(dataGridView);
        private void ZlmButton_Click(object sender, EventArgs e) => Zuglaufmeldung(dataGridView);
        private void ShuntButton_Click(object sender, EventArgs e) => Shunt(dataGridView);

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (var col in dataGridView.Columns)
                    if (!col.IsDisposed)
                        col.Dispose();
            
            base.Dispose(disposing);
        }

        private static class L
        {
            public static readonly string Zlm = T._("&Zuglaufmeldung durch");
            public static readonly string Shunts = T._("&Rangierfahrten");
        }
    }
}