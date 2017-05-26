using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class TimetableEditForm : Form
    {
        private IInfo info;

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        private DataGridView focused;

        public TimetableEditForm()
        {
            InitializeComponent();
        }

        public TimetableEditForm(IInfo info) : this()
        {
            this.info = info;
            info.BackupTimetable();

            topLineLabel.Text = "Züge " + info.Timetable.GetLineName(TOP_DIRECTION);
            bottomLineLabel.Text = "Züge " + info.Timetable.GetLineName(BOTTOM_DIRECTION);

            InitializeGridView(topDataGridView, TOP_DIRECTION);
            InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.T)
                {
                    e.Handled = true;
                    if (ActiveControl == topDataGridView)
                        Trapez(topDataGridView);
                    else if (ActiveControl == bottomDataGridView)
                        Trapez(bottomDataGridView);
                }
            };
        }

        private void InitializeGridView(DataGridView view, TrainDirection direction)
        {
            var stas = info.Timetable.GetStationsOrderedByDirection(direction);
            foreach (var sta in stas)
            {
                if (stas.First() != sta)
                    view.Columns.Add(stas.IndexOf(sta) + "ar", sta.SName + " an");
                if (stas.Last() != sta)
                    view.Columns.Add(stas.IndexOf(sta) + "dp", sta.SName + " ab");
            }

            foreach (var tra in info.Timetable.Trains.Where(t => t.Direction == direction))
            {
                DataGridViewRow trainRow = view.Rows[view.Rows.Add()];

                foreach (var sta in info.Timetable.Stations)
                {
                    var ardp = tra.GetArrDep(sta);
                    var ar = ardp.Arrival.ToShortTimeString();
                    var dp = ardp.Departure.ToShortTimeString();
                    if (ar != "00:00")
                        trainRow.Cells[stas.IndexOf(sta) + "ar"].Value = ar;
                    if (dp != "00:00")
                        trainRow.Cells[stas.IndexOf(sta) + "dp"].Value = dp;
                    if (stas.First() != sta)
                    {
                        var cell = trainRow.Cells[stas.IndexOf(sta) + "ar"];
                        cell.Tag = ardp.TrapeztafelHalt;
                        cell.Style.BackColor = ardp.TrapeztafelHalt ? Color.LightGray : Color.White;
                    }
                }

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.TName };
            }

            foreach (DataGridViewColumn column in view.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            view.GotFocus += (s, e) => focused = view;

            view.SelectionChanged += (s, e) =>
            {
                if (view.SelectedCells.Count != 0)
                {
                    var cell = view.SelectedCells[0];
                    trapeztafelToggle.Enabled = cell.ColumnIndex != 0 && cell.ColumnIndex % 2 != 0;

                    var tr = false;
                    if (cell.Tag != null)
                        tr = (bool)cell.Tag;
                    trapeztafelToggle.Checked = tr;
                }
            };
        }

        private bool UpdateTrainDataFromGrid(Train train, DataGridView view)
        {
            foreach (DataGridViewRow row in view.Rows)
            {
                Train t = (Train)row.Tag;

                if (t != train)
                    continue;

                var stas = info.Timetable.GetStationsOrderedByDirection(train.Direction);
                foreach (var sta in stas)
                {
                    ArrDep ardp = new ArrDep();
                    if (view.Columns.Contains(stas.IndexOf(sta) + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[stas.IndexOf(sta) + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            TimeSpan tsAr = TimeSpan.Parse((string)cellAr.Value);
                            ardp.Arrival = tsAr;
                            if (cellAr.Tag != null)
                                ardp.TrapeztafelHalt = (bool)cellAr.Tag;
                        }
                    }

                    if (view.Columns.Contains(stas.IndexOf(sta) + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[stas.IndexOf(sta) + "dp"];

                        if ((string)cellDp.Value != "" && cellDp.Value != null)
                        {
                            TimeSpan tsDp = TimeSpan.Parse((string)cellDp.Value);
                            ardp.Departure = tsDp;
                            if (cellDp.Tag != null)
                                ardp.TrapeztafelHalt = (bool)cellDp.Tag;
                        }
                    }

                    t.SetArrDep(sta, ardp);
                }
                return true;
            }

            return false;
        }

        private void ValidateCell(DataGridView view, DataGridViewCellValidatingEventArgs e)
        {
            if (e.FormattedValue == null || (string)e.FormattedValue == "")
                return;

            string val = (string)e.FormattedValue;
            if (val.Length == 4 && char.IsDigit(val[0]) && char.IsDigit(val[1]) && char.IsDigit(val[2]) && char.IsDigit(val[3]))
            {
                val = val.Substring(0, 2) + ":" + val.Substring(2, 2);
                view.EditingControl.Text = val;
            }

            if (!TimeSpan.TryParse(val, out TimeSpan ts))
            {
                MessageBox.Show("Formatierungsfehler: Zeit muss im Format hh:mm vorliegen!");
                e.Cancel = true;
            }
        }

        private void Trapez(DataGridView view)
        {
            var cells = view.SelectedCells;
            if (cells.Count == 0)
                return;

            var cell = cells[0];
            if (cell.ColumnIndex == 0 || cell.ColumnIndex % 2 == 0)
                return;

            if (cell.Style.BackColor == Color.LightGray)
            {
                cell.Style.BackColor = Color.White;
                cell.Tag = false; // Halt vor Trapeztafel: Nein
                trapeztafelToggle.Checked = false;
            }
            else
            {
                cell.Style.BackColor = Color.LightGray;
                cell.Tag = true; // Halt vor Trapeztafel: Ja
                trapeztafelToggle.Checked = true;
            }
        }

        private void trapeztafelToggle_Click(object sender, EventArgs e)
        {
            if (focused == topDataGridView)
                Trapez(topDataGridView);
            else if (focused == bottomDataGridView)
                Trapez(bottomDataGridView);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            foreach (var t in info.Timetable.Trains)
            {
                if ((t.Direction == TOP_DIRECTION && !UpdateTrainDataFromGrid(t, topDataGridView))
                    || (t.Direction == BOTTOM_DIRECTION && !UpdateTrainDataFromGrid(t, bottomDataGridView)))
                    throw new Exception("In der Anwendung ist ein interner Fehler aufgetreten!");
            }

            info.ClearBackup();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }

        private void topDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
            => ValidateCell(topDataGridView, e);

        private void bottomDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
            => ValidateCell(bottomDataGridView, e);
    }
}
