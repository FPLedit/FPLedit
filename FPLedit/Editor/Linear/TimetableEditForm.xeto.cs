using Eto.Drawing;
using Eto.Forms;
using FPLedit.Editor.Network;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Linear
{
    internal class TimetableEditForm : Dialog<DialogResult>
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
        private Font fn, fb;

        private TimeNormalizer normalizer;
        private Color errorColor = new Color(Colors.Red, 0.4f);

        public TimetableEditForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            trapeztafelToggle = new ToggleButton(internalToggle);
            trapeztafelToggle.ToggleClick += trapeztafelToggle_Click;

            this.info = info;
            info.BackupTimetable();

            topLineLabel.Text = "Züge " + info.Timetable.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + info.Timetable.GetLineName(BOTTOM_DIRECTION);

            InitializeGridView(topDataGridView, TOP_DIRECTION);
            InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);

            fb = SystemFonts.Bold();
            fn = SystemFonts.Default();
            normalizer = new TimeNormalizer();

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
        }

        private CustomCell GetCell(Func<DataElement, string> text, Station sta, bool arrival, GridView view)
        {
            var mpmode = !Eto.Platform.Instance.SupportedFeatures.HasFlag(Eto.PlatformFeatures.CustomCellSupportsControlView);

            var cc = new CustomCell();
            cc.CreateCell = args => new TextBox();
            cc.ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                var data = (DataElement)args.Item;

                if (!data.HasError(sta, arrival))
                    tb.Text = text(data);

                tb.BackgroundColor = Colors.White;

                var first = data.GetStations(info).First() == sta;
                if (arrival ^ first)
                {
                    if (first && data.ArrDeps[sta].TrapeztafelHalt)
                        throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");

                    tb.BackgroundColor = data.ArrDeps[sta].TrapeztafelHalt ? Colors.LightGrey : Colors.White;
                    if (data.ArrDeps[sta].Zuglaufmeldung != null && data.ArrDeps[sta].Zuglaufmeldung != "")
                        tb.Font = fb;
                    else
                        tb.Font = fn;
                }

                if (data.HasError(sta, arrival))
                    tb.BackgroundColor = errorColor;

                if (mpmode)
                {
                    tb.KeyDown += (s, e) =>
                    {
                        if (e.Key == Keys.Enter)
                        {
                            e.Handled = true;
                            focused.ReloadData(focused.SelectedRow);
                        }
                        else if (e.Key == Keys.T)
                        {
                            e.Handled = true;
                            Trapez(focused);
                        }
                        else if (e.Key == Keys.Z)
                        {
                            e.Handled = true;
                            Zuglaufmeldung(focused);
                        }
                    };
                }

                tb.GotFocus += (s, e) =>
                {
                    CellSelected(data, sta, arrival);
                    data.IsSelectedArrival = arrival;
                    data.SelectedStation = sta;
                    data.SelectedTextBox = tb;
                    focused = view;
                };
                tb.LostFocus += (s, e) => { FormatCell(data, sta, arrival, tb); };
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode)
                    return;

                var t = "";
                var bg = Colors.White;
                var fnt = fn;
                var data = (DataElement)e.Item;

                if (!data.HasError(sta, arrival))
                    t = text(data);

                var first = data.GetStations(info).First() == sta;
                if (arrival ^ first)
                {
                    if (first && data.ArrDeps[sta].TrapeztafelHalt)
                        throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");

                    bg = data.ArrDeps[sta].TrapeztafelHalt ? Colors.LightGrey : Colors.White;
                    if (data.ArrDeps[sta].Zuglaufmeldung != null && data.ArrDeps[sta].Zuglaufmeldung != "")
                        fnt = fb;
                }

                if (data.HasError(sta, arrival))
                    bg = errorColor;

                e.Graphics.Clear(bg);
                e.Graphics.DrawText(fnt, Colors.Black, new PointF(e.ClipRectangle.Left + 2, e.ClipRectangle.Top + 2), t);
                e.Graphics.DrawRectangle(Colors.Black, e.ClipRectangle);
            };
            return cc;
        }

        private void CellSelected(DataElement data, Station sta, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDeps[sta].TrapeztafelHalt;

            internalToggle.Enabled = arrival;
            zlmButton.Enabled = arrival ^ (data.GetStations(info).First() == sta);
        }

        private void ClearSelection()
        {
            trapeztafelToggle.Checked = false;

            internalToggle.Enabled = false;
            zlmButton.Enabled = false;
        }

        private class DataElement
        {
            public Train Train { get; set; }

            public Dictionary<Station, ArrDep> ArrDeps { get; set; }

            public Dictionary<Station, bool?> Errors { get; set; } = new Dictionary<Station, bool?>();

            public bool IsSelectedArrival { get; set; }

            public Station SelectedStation { get; set; }

            public TextBox SelectedTextBox { get; set; }

            public void SetTime(Station sta, bool arrival, string time)
            {
                var a = ArrDeps[sta];
                if (arrival)
                    a.Arrival = TimeSpan.Parse(time);
                else
                    a.Departure = TimeSpan.Parse(time);
                ArrDeps[sta] = a;
            }

            public void SetZlm(Station sta, string zlm)
            {
                var a = ArrDeps[sta];
                a.Zuglaufmeldung = zlm;
                ArrDeps[sta] = a;
            }

            public void SetTrapez(Station sta, bool trapez)
            {
                var a = ArrDeps[sta];
                a.TrapeztafelHalt = trapez;
                ArrDeps[sta] = a;
            }

            public bool HasError(Station sta, bool arrival)
                => Errors.TryGetValue(sta, out bool? val) && val.HasValue && val.Value == arrival;

            public List<Station> GetStations(IInfo info)
                => info.Timetable.GetStationsOrderedByDirection(Train.Direction);
        }

        private void InitializeGridView(GridView view, TrainDirection dir)
        {
            var stations = info.Timetable.GetStationsOrderedByDirection(dir);

            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell() { Binding = Binding.Property<DataElement, string>(t => t.Train.TName) },
                HeaderText = "Zugnummer",
                AutoSize = true,
                Sortable = false,
            });
            foreach (var sta in stations)
            {
                if (sta != stations.First())
                {
                    view.Columns.Add(new GridColumn()
                    {
                        DataCell = GetCell(t => t.ArrDeps[sta].Arrival != default(TimeSpan) ? t.ArrDeps[sta].Arrival.ToShortTimeString() : "", sta, true, view),
                        HeaderText = sta.SName + " an",
                        AutoSize = true,
                        Sortable = false,
                    });
                }
                if (sta != stations.Last())
                {
                    view.Columns.Add(new GridColumn()
                    {
                        DataCell = GetCell(t => t.ArrDeps[sta].Departure != default(TimeSpan) ? t.ArrDeps[sta].Departure.ToShortTimeString() : "", sta, false, view),
                        HeaderText = sta.SName + " ab",
                        AutoSize = true,
                        Sortable = false,
                    });
                }
            }

            var l = info.Timetable.Trains.Where(t => t.Direction == dir).Select(tra => new DataElement()
            {
                Train = tra,
                ArrDeps = tra.GetArrDeps()
            }).ToList();

            //TODO: Hier eine neue Idee...
            //view.LostFocus += (s, e) => ClearSelection();

            view.GotFocus += (s, e) => focused = view;

            view.DataStore = l;
        }

        private void FormatCell(DataElement data, Station sta, bool arrival, TextBox tb)
        {
            string val = tb.Text;
            if (val == null || val == "")
            {
                data.Errors[sta] = null;
                data.SetTime(sta, arrival, "0");
                return;
            }

            val = normalizer.Normalize(val);
            bool error = true;
            if (val != null)
            {
                tb.Text = val;
                data.SetTime(sta, arrival, val);
                error = false;
            }
            data.Errors[sta] = error ? (bool?)arrival : null;
        }

        private void Trapez(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement)view.SelectedItem;

            // Trapeztafelhalt darf nur bei Ankünften sein
            if (!data.IsSelectedArrival)
                return;

            var sta = data.SelectedStation;
            var trapez = !data.ArrDeps[sta].TrapeztafelHalt;
            data.SetTrapez(sta, trapez);

            view.ReloadData(view.SelectedRow);
            trapeztafelToggle.Checked = trapez;
            CellSelected(data, sta, data.IsSelectedArrival);
        }

        private void Zuglaufmeldung(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement)view.SelectedItem;
            var sta = data.SelectedStation;

            // Zuglaufmeldungen dürfen auch bei Abfahrt am ersten Bahnhof sein
            if (!(data.GetStations(info).First() == sta) && !data.IsSelectedArrival)
                return;

            var zlmDialog = new ZlmEditForm(data.ArrDeps[sta].Zuglaufmeldung ?? "");
            if (zlmDialog.ShowModal(this) != DialogResult.Ok)
                return;

            data.SetZlm(sta, zlmDialog.Zlm);

            view.ReloadData(view.SelectedRow);
        }

        private bool UpdateTrainDataFromGrid(Train train, GridView view)
        {
            foreach (DataElement row in view.DataStore)
            {
                if (row.Train != train)
                    continue;

                if (row.Errors.Any(e => e.Value.HasValue))
                {
                    MessageBox.Show("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
                    return false;
                }

                foreach (var sta in row.GetStations(info))
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
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }

        #region Events
        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => ViewDependantAction(Trapez);

        private void zlmButton_Click(object sender, EventArgs e)
            => ViewDependantAction(Zuglaufmeldung);
        #endregion
    }
}
