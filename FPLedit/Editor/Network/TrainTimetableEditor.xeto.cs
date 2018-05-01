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
            var cc = new CustomCell();
            cc.CreateCell = args => new TextBox();
            cc.ConfigureCell = (args, control) =>
            {
                var tb = (TextBox)control;
                var data = (DataElement)args.Item;
                tb.Text = text(data);

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
                    tb.BackgroundColor = Colors.PaleVioletRed;

                tb.GotFocus += (s, e) => { CellSelected(data, arrival); data.IsSelectedArrival = arrival; data.SelectedTextBox = tb; };
                tb.LostFocus += (s, e) => { FormatCell(data, arrival, tb); };
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

            var l = path.Select(sta => new DataElement()
            {
                Station = sta,
                ArrDep = train.GetArrDep(sta),
                IsFirst = path.First() == sta,
                IsLast = path.Last() == sta,
            }).ToList();

            //TODO: Hier eine neue Idee...
            //view.LostFocus += (s, e) => ClearSelection();

            view.DataStore = l;
        }

        private void FormatCell(DataElement data, bool arrival, TextBox tb)
        {
            string val = tb.Text;
            if (val == null || val == "")
                return;

            val = normalizer.Normalize(val);
            if (val != null)
            {
                tb.Text = val;
                data.SetTime(arrival, val);
            }
            else
            {
                MessageBox.Show("Formatierungsfehler: Zeit muss im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
                if (arrival)
                    data.IsArrivalError = true;
                else
                    data.IsDepartureError = true;
            }
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
                    MessageBox.Show("Bitte erst alle Fehler beheben!");
                    return false;
                }

                //throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");
                //throw new Exception("Keine Abfahrtszelle darf einen Trapeztafelhalt/Zugalufmeldungseintrag enthalten!");

                train.SetArrDep(row.Station, row.ArrDep);
            }
            return true;
        }

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

        #region Events
        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => Trapez(dataGridView);

        private void zlmButton_Click(object sender, EventArgs e)
            => Zuglaufmeldung(dataGridView);
        #endregion
    }
}
