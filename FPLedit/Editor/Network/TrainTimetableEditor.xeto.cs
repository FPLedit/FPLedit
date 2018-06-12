using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.Network
{
    internal class TrainTimetableEditor : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private GridView dataGridView;
        private Button internalToggle;
        private ToggleButton trapeztafelToggle;
        private Button zlmButton;
#pragma warning restore CS0649

        private IInfo info;
        private Train train;
        private List<Station> path;

        private Font fn, fb;

        private TimeNormalizer normalizer;
        private Color errorColor = new Color(Colors.Red, 0.4f);

        private bool mpmode = false;

        private TrainTimetableEditor()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            trapeztafelToggle = new ToggleButton(internalToggle);
            trapeztafelToggle.ToggleClick += trapeztafelToggle_Click;

            fb = SystemFonts.Bold();
            fn = SystemFonts.Default();
            normalizer = new TimeNormalizer();

            KeyDown += (s, e) =>
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
            };

            internalToggle.Image = new Bitmap(this.GetResource("Resources.trapeztafel.png"));

            mpmode = !Eto.Platform.Instance.SupportedFeatures.HasFlag(Eto.PlatformFeatures.CustomCellSupportsControlView);

            if (mpmode)
                DefaultButton = null; // Bugfix, Window closes on enter [Enter]
        }

        public TrainTimetableEditor(IInfo info, Train t) : this()
        {
            this.info = info;
            info.BackupTimetable();

            train = t;
            path = t.GetPath();
            Title = Title.Replace("{train}", t.TName);

            InitializeGridView(dataGridView);
        }

        private CustomCell GetCell(Func<DataElement, string> text, bool arrival)
        {
            var mpmode = !Eto.Platform.Instance.SupportedFeatures.HasFlag(Eto.PlatformFeatures.CustomCellSupportsControlView);

            var cc = new CustomCell();
            cc.CreateCell = args => new TextBox();
            cc.ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                var data = (DataElement)args.Item;

                if ((arrival && !data.IsArrivalError) || (!arrival && !data.IsDepartureError))
                    tb.Text = text(data);

                tb.BackgroundColor = Colors.White;

                if ((!arrival && data.IsLast) || (arrival && data.IsFirst))
                {
                    tb.ReadOnly = true;
                    tb.BackgroundColor = Colors.DarkGray;
                }
                else if (arrival ^ data.IsFirst)
                {
                    if (data.IsFirst && data.ArrDep.TrapeztafelHalt)
                        throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");

                    tb.BackgroundColor = data.ArrDep.TrapeztafelHalt ? Colors.LightGrey : Colors.White;
                    if (data.ArrDep.Zuglaufmeldung != null && data.ArrDep.Zuglaufmeldung != "")
                        tb.Font = fb;
                    else
                        tb.Font = fn;
                }

                if ((arrival && data.IsArrivalError) || (!arrival && data.IsDepartureError))
                    tb.BackgroundColor = errorColor;

                if (mpmode)
                {
                    tb.KeyDown += (s, e) => HandleKeystroke(e, dataGridView);

                    // Wir gehen hier gleich in den vollen EditMode rein
                    tb.CaretIndex = 0;
                    tb.SelectAll();
                    CellSelected(data, arrival); data.IsSelectedArrival = arrival; data.SelectedTextBox = tb;
                }

                tb.GotFocus += (s, e) => { CellSelected(data, arrival); data.IsSelectedArrival = arrival; data.SelectedTextBox = tb; };
                tb.LostFocus += (s, e) => { FormatCell(data, arrival, tb); };
            };
            cc.Paint += (s, e) =>
            {
                if (!mpmode)
                    return;

                var t = "";
                var bg = Colors.White;
                var fnt = fn;
                var data = (DataElement)e.Item;

                if ((arrival && !data.IsArrivalError) || (!arrival && !data.IsDepartureError))
                    t = text(data);

                if ((!arrival && data.IsLast) || (arrival && data.IsFirst))
                    bg = Colors.DarkGray;
                else if (arrival ^ data.IsFirst)
                {
                    if (data.IsFirst && data.ArrDep.TrapeztafelHalt)
                        throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");

                    bg = data.ArrDep.TrapeztafelHalt ? Colors.LightGrey : Colors.White;
                    if (data.ArrDep.Zuglaufmeldung != null && data.ArrDep.Zuglaufmeldung != "")
                        fnt = fb;
                }

                if ((arrival && data.IsArrivalError) || (!arrival && data.IsDepartureError))
                    bg = errorColor;

                e.Graphics.Clear(bg);
                e.Graphics.DrawText(fnt, Colors.Black, new PointF(e.ClipRectangle.Left + 2, e.ClipRectangle.Top + 2), t);
                e.Graphics.DrawRectangle(Colors.Black, e.ClipRectangle);
            };
            return cc;
        }

        private void CellSelected(DataElement data, bool arrival)
        {
            trapeztafelToggle.Checked = data.ArrDep.TrapeztafelHalt;

            internalToggle.Enabled = arrival && !data.IsFirst;
            zlmButton.Enabled = arrival ^ data.IsFirst;
        }

        private void ClearSelection()
        {
            trapeztafelToggle.Checked = false;

            internalToggle.Enabled = false;
            zlmButton.Enabled = false;
        }

        private class DataElement
        {
            public Station Station { get; set; }

            public ArrDep ArrDep { get; set; }

            public bool IsFirst { get; set; }

            public bool IsLast { get; set; }

            public bool IsArrivalError { get; set; }

            public bool IsDepartureError { get; set; }

            public bool IsSelectedArrival { get; set; }

            public TextBox SelectedTextBox { get; set; }

            public void SetTime(bool arrival, string time)
            {
                var a = ArrDep;
                if (arrival)
                    a.Arrival = TimeSpan.Parse(time);
                else
                    a.Departure = TimeSpan.Parse(time);
                ArrDep = a;
            }

            public void SetZlm(string zlm)
            {
                var a = ArrDep;
                a.Zuglaufmeldung = zlm;
                ArrDep = a;
            }

            public void SetTrapez(bool trapez)
            {
                var a = ArrDep;
                a.TrapeztafelHalt = trapez;
                ArrDep = a;
            }
        }

        private void InitializeGridView(GridView view)
        {
            view.Columns.Add(new GridColumn()
            {
                DataCell = new TextBoxCell() { Binding = Binding.Property<DataElement, string>(t => t.Station.SName) },
                HeaderText = "Bahnhof",
                AutoSize = true,
                Sortable = false,
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = GetCell(t => t.ArrDep.Arrival != default(TimeSpan) ? t.ArrDep.Arrival.ToShortTimeString() : "", true),
                HeaderText = "Ankunft",
                AutoSize = true,
                Sortable = false,
            });
            view.Columns.Add(new GridColumn()
            {
                DataCell = GetCell(t => t.ArrDep.Departure != default(TimeSpan) ? t.ArrDep.Departure.ToShortTimeString() : "", false),
                HeaderText = "Abfahrt",
                AutoSize = true,
                Sortable = false,
            });

            view.KeyDown += (s, e) =>
            {
                if (e.Key == Keys.Home) // Pos1
                {
                    if (!mpmode)
                        return;

                    e.Handled = true;
                    if (view.IsEditing)
                        view.ReloadData(view.SelectedRow);
                    view.BeginEdit(0, 2); // erstes Abfahrtsfeld (Ankunft ja deaktiviert)
                }
                else
                    HandleKeystroke(e, view);
            };

            var l = path.Select(sta => new DataElement()
            {
                Station = sta,
                ArrDep = train.GetArrDep(sta),
                IsFirst = path.First() == sta,
                IsLast = path.Last() == sta,
            }).ToList();

            view.DataStore = l;
        }

        private void HandleKeystroke(KeyEventArgs e, GridView view)
        {
            if (view == null)
                return;

            if (e.Key == Keys.Enter)
            {
                if (!mpmode)
                    return;

                e.Handled = true;
                var data = (DataElement)view.SelectedItem;
                if (data == null || data.SelectedTextBox == null)
                    return;
                FormatCell(data, data.IsSelectedArrival, data.SelectedTextBox);
                view.ReloadData(view.SelectedRow);
            }
            else if (e.Key == Keys.T)
            {
                e.Handled = true;
                Trapez(view);
            }
            else if (e.Key == Keys.Z)
            {
                e.Handled = true;
                Zuglaufmeldung(view);
            }
            else if (e.Key == Keys.Tab)
            {
                if (!mpmode)
                    return;

                e.Handled = true;
                var data = (DataElement)view.SelectedItem;
                if (data == null || data.SelectedTextBox == null)
                    return;
                FormatCell(data, data.IsSelectedArrival, data.SelectedTextBox);

                var arrival = data.IsSelectedArrival;
                var idx = arrival ? 2 : 1;
                int row;
                if (!e.Control)
                    row = view.SelectedRow + (arrival ? 0 : 1);
                else
                    row = view.SelectedRow - (arrival ? 1 : 0);

                view.ReloadData(view.SelectedRow); // Commit current data
                view.BeginEdit(row, idx);
            }
            else
            {
                if (!mpmode)
                    return;

                var data = (DataElement)view.SelectedItem;
                if (data == null || data.SelectedTextBox == null)
                    return;
                var tb = data.SelectedTextBox;
                if (tb.HasFocus) // Wir können "echt" editieren
                    return;
                if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar))
                {
                    tb.Text += e.KeyChar;
                    e.Handled = true;
                }
                if (e.Key == Keys.Backspace && tb.Text.Length > 0)
                    tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
            }
        }

        private void FormatCell(DataElement data, bool arrival, TextBox tb)
        {
            string val = tb.Text;
            if (val == null || val == "")
            {
                if (arrival)
                    data.IsArrivalError = false;
                else
                    data.IsDepartureError = false;
                data.SetTime(arrival, "0");
                return;
            }

            val = normalizer.Normalize(val);
            bool error = true;
            if (val != null)
            {
                tb.Text = val;
                data.SetTime(arrival, val);
                error = false;
            }

            if (arrival)
                data.IsArrivalError = error;
            else
                data.IsDepartureError = error;
        }

        private void Trapez(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement)view.SelectedItem;

            // Trapeztafelhalt darf nur bei Ankünften sein
            if (!data.IsSelectedArrival)
                return;

            var trapez = !data.ArrDep.TrapeztafelHalt;
            data.SetTrapez(trapez);

            view.ReloadData(view.SelectedRow);
            trapeztafelToggle.Checked = trapez;
            CellSelected(data, data.IsSelectedArrival);
        }

        private void Zuglaufmeldung(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (DataElement)view.SelectedItem;

            // Zuglaufmeldungen dürfen auch bei Abfahrt am ersten Bahnhof sein
            if (!data.IsFirst && !data.IsSelectedArrival)
                return;

            var zlmDialog = new ZlmEditForm(data.ArrDep.Zuglaufmeldung ?? "");
            if (zlmDialog.ShowModal(this) != DialogResult.Ok)
                return;

            data.SetZlm(zlmDialog.Zlm);

            view.ReloadData(view.SelectedRow);
        }

        private bool UpdateTrainDataFromGrid(GridView view)
        {
            foreach (DataElement row in view.DataStore)
            {
                if (row.IsArrivalError || row.IsDepartureError)
                {
                    MessageBox.Show("Bitte erst alle Fehler beheben!\n\nDie Zeitangaben müssen im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
                    return false;
                }

                train.SetArrDep(row.Station, row.ArrDep);
            }
            return true;
        }

        #region Events
        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            if (!UpdateTrainDataFromGrid(dataGridView))
                return;

            info.ClearBackup();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }

        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => Trapez(dataGridView);

        private void zlmButton_Click(object sender, EventArgs e)
            => Zuglaufmeldung(dataGridView);
        #endregion
    }
}
