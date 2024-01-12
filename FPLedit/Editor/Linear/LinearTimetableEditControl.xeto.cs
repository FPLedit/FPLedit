using Eto.Drawing;
using Eto.Forms;
using FPLedit.Editor.TimetableEditor;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;

namespace FPLedit.Editor.Linear;

internal sealed class LinearTimetableEditControl : BaseTimetableEditControl
{
    private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
    private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

#pragma warning disable CS0649,CA2213
    private readonly GridView topDataGridView = default!, bottomDataGridView = default!;
    private readonly Label topLineLabel = default!, bottomLineLabel = default!;
    private readonly ToggleButton trapeztafelToggle = default!;
    private readonly Button zlmButton = default!;
    private readonly TableLayout actionsLayout = default!;
#pragma warning restore CS0649,CA2213

    private GridView? focused;

    private Timetable tt = null!;

    protected override int FirstEditingColumn => 1; // erstes Abfahrtsfeld

    public LinearTimetableEditControl()
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        trapeztafelToggle.Click += (_, _) => ViewDependantAction(Trapez);
        zlmButton.Click += (_, _) => ViewDependantAction(Zuglaufmeldung);
        Init(trapeztafelToggle, actionsLayout);

        KeyDown += (_, e) => HandleControlKeystroke(e);

        trapeztafelToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png"));
    }

    public void HandleControlKeystroke(KeyEventArgs e)
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

        var rt = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);
        topLineLabel.Text = T._("Züge {0}", rt.GetRouteName(TOP_DIRECTION.IsSortReverse()));
        bottomLineLabel.Text = T._("Züge {0}", rt.GetRouteName(BOTTOM_DIRECTION.IsSortReverse()));

        InitializeGridView(topDataGridView, TOP_DIRECTION);
        InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);

        Initialized = true;
    }

    private CustomCell GetCell(Func<ArrDep, TimeEntry> time, Station sta, bool arrival, GridView view)
    {
        void TbEnterEditMode(DataElement data, TextBox tb)
        {
            CellSelected(data, sta, arrival);
            data.IsSelectedArrival = arrival;
            data.SelectedStation = sta;
            data.SelectedTextBox = tb;
            focused = view;
        }

        var cc = new CustomCell
        {
            CreateCell = _ =>
            {
                var tb = new TextBox { Tag = new CCCO() };
                    
                if (mpmode)
                    tb.KeyDown += (_, e) =>
                    {
                        focused = view;
                        HandleControlKeystroke(e);
                    };
                    
                tb.GotFocus += (s, _) =>
                {
                    var dd2 = (TextBox) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    TbEnterEditMode((DataElement)ccco.Data, dd2);
                };
                    
                tb.LostFocus += (s, _) =>
                {
                    var dd2 = (TextBox) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    FormatCell(ccco.Data, sta, arrival, tb); 
                    new TimetableCellRenderProperties(time, sta, arrival, ccco.Data).Apply(tb);
                };
                    
                return tb;
            },
            ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                if (tb == null)
                    return;
                var ccco = (CCCO) tb.Tag;
                ccco.InhibitEvents = true; // Inihibit event handling while new state is set.
                    
                var data = (DataElement)args.Item;
                if (data == null)
                {
                    tb.Visible = false;
                    return;
                }
                if (data.IsMpDummy) return; // Skip "last rows" in mpmode.
                ccco.Data = data;

                new TimetableCellRenderProperties(time, sta, arrival, data).Apply(tb);

                ccco.InhibitEvents = false;

                if (mpmode)
                {
                    // Enter the full edit mode.
                    tb.CaretIndex = 0;
                    TbEnterEditMode(data, tb);
                    Application.Instance.InvokeAsync(() =>
                    {
                        tb.SelectAll();
                        if (tb.Enabled) tb.Focus();
                        else view.Focus();
                    });
                }
            }
        };
        cc.Paint += (_, e) =>
        {
            if (!mpmode)
                return;

            var data = (DataElement)e.Item;
            if (data == null || data.IsMpDummy) return;

            new TimetableCellRenderProperties(time, sta, arrival, data).Render(e.Graphics, e.ClipRectangle);
        };
        return cc;
    }

    protected override (int col, int row) GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e)
    {
        var path = data.Train!.GetPath();
        int idx, row;
        if (!e.Control)
        {
            idx = path.IndexOf(data.GetStation()!) * 2 + (data.IsSelectedArrival ? 1 : 2);
            row = view.SelectedRow;
            if (path.Last() == data.GetStation() && data.IsSelectedArrival)
            {
                idx = 1;
                row++;
            }
        }
        else
        {
            idx = path.IndexOf(data.GetStation()!) * 2 - (data.IsSelectedArrival ? 1 : 0);
            row = view.SelectedRow;
            if (path.First() == data.GetStation() && !data.IsSelectedArrival)
            {
                idx = (path.Count - 1) * 2;
                row--;
            }
        }
        return (idx, row);
    }

    protected override void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival)
    {
        trapeztafelToggle.Checked = data.ArrDeps![sta].TrapeztafelHalt;

        trapeztafelToggle.Enabled = arrival;
        zlmButton.Enabled = arrival ^ (data.IsFirst(sta));
    }

    private class DataElement : BaseTimetableDataElement
    {
        public Station? SelectedStation { get; set; }

        public override Station? GetStation() => SelectedStation;

        public static DataElement CreateMpDummy()
        {
            return new()
            {
                IsMpDummy = true,
                Train = null,
                ArrDeps = null,
                SelectedStation = null,
            };
        }
    }

    private string GetTransition(ITrain t) => tt.GetTransition(t)?.TName ?? "";

    private void InitializeGridView(GridView view, TrainDirection dir)
    {
        var stations = tt.GetRoute(Timetable.LINEAR_ROUTE_ID).Stations.ToList().MaybeReverseDirection(dir);

        view.AddFuncColumn<DataElement>(t => t.IsMpDummy ? "" : t.Train!.TName, T._("Zugnummer"));
        foreach (var sta in stations)
        {
            if (sta != stations.First())
                view.AddColumn(GetCell(t => t.Arrival, sta, true, view), T._("{0} an", sta.SName), editable: true);
            if (sta != stations.Last())
                view.AddColumn(GetCell(t => t.Departure, sta, false, view), T._("{0} ab", sta.SName), editable: true);
        }
        view.AddFuncColumn<DataElement>(t => GetTransition(t.Train!), T._("Folgezug"));

        view.GotFocus += (_, _) => focused = view;
        view.KeyDown += (_, e) => HandleViewKeystroke(e, view);

        var l = tt.Trains.Where(t => t.Direction == dir).Select(tra => new DataElement
        {
            Train = tra,
            ArrDeps = tra.GetArrDepsUnsorted()
        }).ToArray();
        //if (mpmode)
        //    l.Add(DataElement.CreateMpDummy()); // Add empty "last line" in multiplatform mode.

        view.DataStore = l;

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) view.AllowEmptySelection = false;
    }

    private bool UpdateTrainDataFromGrid(ITrain train, GridView view)
    {
        foreach (DataElement row in view.DataStore)
        {
            if (row.Train != train)
                continue;

            if (row.HasAnyError)
            {
                MessageBox.Show(T._("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!"));
                return false;
            }

            return true;
        }

        throw new Exception(T._("Zug, der am Anfang noch da war, ist verschwunden!"));
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var col in topDataGridView.Columns)
                if (!col.IsDisposed)
                    col.Dispose();
            foreach (var col in bottomDataGridView.Columns)
                if (!col.IsDisposed)
                    col.Dispose();
        }

        base.Dispose(disposing);
    }

    private static class L
    {
        public static readonly string Zlm = T._("&Zuglaufmeldung durch");
        public static readonly string TrapezTooltip = T._("Halt vor Trapeztafel");
        public static readonly string Shunts = T._("&Rangierfahrten");
    }
}