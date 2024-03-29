﻿using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.TimetableEditor;

internal sealed class SingleTimetableEditControl : BaseTimetableEditControl
{
#pragma warning disable CS0649,CA2213
    private readonly GridView dataGridView = default!;
    private readonly ToggleButton trapeztafelToggle = default!, requestToggle = default!;
    private readonly Button zlmButton = default!, shuntButton = default!;
    private readonly TableLayout actionsLayout = default!;
#pragma warning restore CS0649,CA2213

    private IWritableTrain train = null!;
    private List<Station> path = null!;

    private const string NO_TRACK = "<Kein Gleis>";

    protected override int FirstEditingColumn => 2; // erstes Abfahrtsfeld (Ankunft ja deaktiviert)

    public SingleTimetableEditControl()
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        trapeztafelToggle.Click += (_, _) => Trapez(dataGridView);
        requestToggle.Click += (_, _) => RequestStop(dataGridView);
        zlmButton.Click += (_, _) => Zuglaufmeldung(dataGridView);
        shuntButton.Click += (_, _) => Shunt(dataGridView);
        Init(trapeztafelToggle, actionsLayout);

        KeyDown += (_, e) => HandleControlKeystroke(e);
        dataGridView.KeyDown += (_, e) => HandleControlKeystroke(e);

#pragma warning disable CA2000
        trapeztafelToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png")).WithSize(50, 30);
        requestToggle.Image = new Bitmap(this.GetResource("Resources.bedarf.png")).WithSize(50, 30);
#pragma warning disable CA2000
    }

    public bool HandleControlKeystroke(KeyEventArgs e)
    {
        bool handled = true;
        switch (e.Key)
        {
            case Keys.T: Trapez(dataGridView); break;
            case Keys.B: RequestStop(dataGridView); break;
            case Keys.Z: Zuglaufmeldung(dataGridView); break;
            case Keys.R: Shunt(dataGridView); break;
            default:
                handled = false;
                break;
        }

        if (handled)
            e.Handled = true;
        else
            HandleKeystroke(e, dataGridView);
        return handled;
    }

    public void Initialize(IWritableTrain t)
    {
        train = t;
        path = t.GetPath();

        InitializeGridView(dataGridView);
        Initialized = true;
    }

    private CustomCell GetCell(Func<ArrDep, TimeEntry> time, bool arrival)
    {
        var cc = new CustomCell
        {
            CreateCell = _ =>
            {
                var tb = new TextBox { Tag = new CCCO() };

                if (mpmode)
                    tb.KeyDown += (s, e) =>
                    {
                        var hadCellAction = HandleControlKeystroke(e);
                        if (!hadCellAction || s != tb) return;

                        // Return to current editing position.
                        var ccco = (CCCO) tb.Tag;
                        if (ccco.InhibitEvents)
                            return;
                        var cep = GetCurEditingPosition(ccco.Data, dataGridView);
                        dataGridView.BeginEdit(cep.row, cep.col);
                    };

                tb.GotFocus += (s, _) =>
                {
                    var dd2 = (TextBox) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    CellSelected(ccco.Data, ccco.Data.GetStation()!, arrival);
                    ccco.Data.IsSelectedArrival = arrival;
                    ccco.Data.SelectedTextBox = tb;
                };

                tb.LostFocus += (s, _) =>
                {
                    var dd2 = (TextBox) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    FormatCell(ccco.Data, ccco.Data.GetStation()!, arrival, tb);
                    new TimetableCellRenderProperties(time, ccco.Data.GetStation()!, arrival, ccco.Data).Apply(tb);
                };

                return tb;
            },
            ConfigureCell = (args, control) =>
            {
                var tb = (TextBox) control;
                if (tb == null) return;
                var ccco = (CCCO) tb.Tag;
                ccco.InhibitEvents = true;

                if (args?.Item == null) return;

                var data = (DataElement) args.Item;
                if (data.IsMpDummy) return; // Skip "last rows" in mpmode.

                ccco.Data = data;
                new TimetableCellRenderProperties(time, data.Station!, arrival, data).Apply(tb);

                ccco.InhibitEvents = false;

                if (mpmode)
                {
                    // Enter the full edit mode.
                    tb.CaretIndex = 0;
                    CellSelected(data, data.Station!, arrival);
                    data.IsSelectedArrival = arrival;
                    data.SelectedTextBox = tb;

                    Application.Instance.InvokeAsync(() =>
                    {
                        tb.SelectAll();
                        if (tb.Enabled) tb.Focus();
                        else dataGridView.Focus();
                    });
                }
            }
        };
        cc.Paint += (_, e) =>
        {
            if (!mpmode) return;
            var data = (DataElement) e.Item;
            if (data.IsMpDummy) return; // Skip "last rows" in mpmode.
            new TimetableCellRenderProperties(time, data.Station!, arrival, data).Render(e.Graphics, e.ClipRectangle);
        };
        return cc;
    }

    private CustomCell GetTrackCell(Func<ArrDep, string> track, Action<ArrDep, string> setTrack, bool arrival)
    {
        var cc = new CustomCell
        {
            CreateCell = _ =>
            {
                var dd = new DropDown { Tag = new CCCO() };

                dd.SelectedIndexChanged += (s, _) =>
                {
                    var dd2 = (DropDown) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    var t = (string) dd.SelectedValue;
                    //var ds = ((DataElement)ccco.Data).GetTrackDataStore().ToArray();

                    setTrack(ccco.Data.ArrDeps![ccco.Data.GetStation()!], t == (string?) dd.DataStore.FirstOrDefault() ? "" : t);
                };

                dd.GotFocus += (s, _) =>
                {
                    var dd2 = (DropDown) s!;
                    var ccco = (CCCO) dd2.Tag;
                    if (ccco.InhibitEvents)
                        return;
                    ccco.Data.IsSelectedArrival = arrival;
                    ccco.Data.SelectedTextBox = null;
                    ccco.Data.SelectedDropDown = dd;
                };

                dd.KeyDown += (_, e) => HandleKeystroke(e, dataGridView);

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
                if (data.IsMpDummy) return; // Skip "last rows" in mpmode.
                ccco.Data = data;

                var ds = data.GetTrackDataStore().ToArray();

                dd.DataStore = ds;
                dd.Enabled = ds.Length > 1;
                if (data.IsFirst(data.Station!) && arrival || data.IsLast(data.Station!) && !arrival)
                    dd.Enabled = false;

                dd.SelectedValue = track(data.ArrDeps![data.Station!]);

                ccco.InhibitEvents = false; // Resume event handling of cutom control.

                if (mpmode)
                {
                    data.IsSelectedArrival = arrival;
                    data.SelectedTextBox = null;
                    data.SelectedDropDown = dd;
                    dd.Focus();
                    Application.Instance.InvokeAsync(() =>
                    {
                        if (dd.Enabled) dd.Focus();
                        else dataGridView.Focus();
                    });
                }
            },
        };
        cc.Paint += (_, e) =>
        {
            if (!mpmode) return;
            var data = (DataElement) e.Item;
            if (data.IsMpDummy) return; // Skip "last rows" in mpmode.
            new TimetableCellRenderProperties2(track(data.ArrDeps![data.Station!])).Render(e.Graphics, e.ClipRectangle);
        };
        return cc;
    }

    private CheckBoxCell GetCheckCell(Func<ArrDep, bool> check)
    {
        bool? B(DataElement d)
        {
            if (d.IsMpDummy) return null;
            return check(d.ArrDeps![d.Station!]);
        }
        return new CheckBoxCell { Binding = Binding.Delegate<DataElement, bool?>(B) };
    }

    protected override void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival)
    {
        if (data.IsMpDummy) return; // Skip "last rows" in mpmode.

        trapeztafelToggle.Checked = data.ArrDeps![sta].TrapeztafelHalt;
        requestToggle.Checked = data.ArrDeps[sta].RequestStop || sta.RequestStop;

        trapeztafelToggle.Enabled = requestToggle.Enabled = arrival && !data.IsFirst(sta) && !sta.RequestStop;
        zlmButton.Enabled = arrival ^ data.IsFirst(sta);
    }

    private class DataElement : BaseTimetableDataElement
    {
        public Station? Station { get; }

        public override Station? GetStation() => Station;

        public IEnumerable<object> GetTrackDataStore()
        {
            var ds = Station!.Tracks.Select(s => s.Name).ToList();
            ds.Insert(0, NO_TRACK);
            return ds;
        }

        public void SetRequestStop(Station sta, bool req)
        {
            if (sta.RequestStop)
                return;
            var a = ArrDeps![sta];
            a.RequestStop = req;
            ArrDeps[sta] = a;
        }

        public DataElement(ITrain tra, Station? sta, ArrDep arrDep)
        {
            Train = tra;
            if (sta != null)
                ArrDeps = new Dictionary<Station, ArrDep>(1) { [sta] = arrDep };
            Station = sta;
        }
    }

    private void InitializeGridView(GridView view)
    {
        var pathData = new PathData(train.ParentTimetable, path);
        var posAlongPath = pathData.GetPositionsAlongPath();

        string NameCell(DataElement t)
        {
            if (t.IsMpDummy) return "";

            // Durchlaufende Kilometrierung.
            posAlongPath.TryGetValue(t.Station!, out var pos);
            return t.Station!.SName + $" [{pos:f1}]";
        }

        view.Columns.Clear();
        view.AddFuncColumn<DataElement>(NameCell, T._("Bahnhof"));
        view.AddColumn(GetCell(t => t.Arrival, true), T._("Ankunft"), editable: true);
        view.AddColumn(GetCell(t => t.Departure, false), T._("Abfahrt"), editable: true);
        view.AddColumn(GetTrackCell(t => t.ArrivalTrack, (t,s) => t.ArrivalTrack = s, true), T._("Ankunftsgleis"), editable: true);
        view.AddColumn(GetTrackCell(t => t.DepartureTrack, (t,s) => t.DepartureTrack = s, false), T._("Abfahrtsgleis"), editable: true);
        view.AddColumn(GetCheckCell(t => t.ShuntMoves.Any()), T._("Rangiert"), editable: false);

        view.KeyDown += (_, e) => HandleViewKeystroke(e, view);

        view.DataStore = path.Select(sta => new DataElement(train, sta, train.GetArrDep(sta))).ToArray();
        view.SelectedRowsChanged += (_, _) =>
        {
            var selected = (DataElement?) view.SelectedItem;
            if (selected == null || selected.IsMpDummy)
                shuntButton.Enabled = false;
            else
                shuntButton.Enabled = selected.Station?.Tracks.Any() ?? false;
        };

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) view.AllowEmptySelection = false;
    }

    private (int col, int row) GetCurEditingPosition(BaseTimetableDataElement data, GridView view)
    {
        var arrival = data.IsSelectedArrival;
        var isTime = data.SelectedTextBox == null;
        int col = (arrival ? 1 : 2) + (isTime ? 2 : 0);
        int row = view.SelectedRow;
        return (col, row);
    }

    protected override (int col, int row) GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e)
    {
        var (idx, row) = GetCurEditingPosition(data, view);
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

        return (idx, row);
    }

    private bool UpdateTrainDataFromGrid(GridView view)
    {
        var hasError = view.DataStore.Cast<DataElement>().Any(row => row.HasAnyError);
        if (hasError)
            MessageBox.Show(T._("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!"));
        return !hasError;
    }

    private void Shunt(GridView view)
    {
        if (view.SelectedRow == -1)
            return;

        var data = (DataElement?) view.SelectedItem;
        if (data == null || data.IsMpDummy) return;
        var sta = data.Station!;

        if (!sta.Tracks.Any()) // We have no tracks.
            return;

        var arrDep = data.ArrDeps![sta];

        using (var shf = new ShuntForm(train, arrDep, sta))
            if (shf.ShowModal(this) != DialogResult.Ok)
                return;

        view.ReloadData(view.SelectedRow);
    }

    private void RequestStop(GridView view)
    {
        if (view.SelectedRow == -1)
            return;

        var data = (DataElement?) view.SelectedItem;
        if (data == null || data.IsMpDummy) return;

        // Request Stop darf nur bei Ankünften sein
        if (!data.IsSelectedArrival)
            return;

        var sta = data.GetStation()!;

        // All traisn stop at this station.
        if (sta.RequestStop)
            return;

        var req = !data.ArrDeps![sta].RequestStop;
        data.SetRequestStop(sta, req);

        view.ReloadData(view.SelectedRow);
        requestToggle.Checked = req;
        CellSelected(data, sta, data.IsSelectedArrival);
    }

    public bool ApplyChanges() => UpdateTrainDataFromGrid(dataGridView);

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
        public static readonly string TrapezTooltip = T._("Halt vor Trapeztafel");
        public static readonly string RequestTooltip = T._("Bedarfshalt");
    }
}