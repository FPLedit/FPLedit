using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor.Network
{
    public partial class TrainTimetableEditor : Form
    {
        private IInfo info;
        private Train train;
        private List<Station> path;

        private DataGridViewCellStyle disabledStyle;
        private Font fn, fb;

        private Editor.TimeNormalizer normalizer;

        private TrainTimetableEditor()
        {
            InitializeComponent();

            disabledStyle = new DataGridViewCellStyle()
            {
                SelectionBackColor = Color.DarkGray,
                BackColor = Color.DarkGray,
            };
            fb = new Font(DefaultFont, FontStyle.Bold);
            fn = new Font(DefaultFont, FontStyle.Regular);
            normalizer = new Editor.TimeNormalizer();

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.T)
                {
                    e.Handled = true;
                    Trapez(dataGridView);
                }
                else if (e.KeyCode == Keys.Z)
                {
                    e.Handled = true;
                    Zuglaufmeldung(dataGridView);
                }
            };
        }

        public TrainTimetableEditor(IInfo info, Train t) : this()
        {
            this.info = info;
            info.BackupTimetable();

            train = t;
            path = t.GetPath();

            InitializeGridView(dataGridView);
        }

        private void InitializeGridView(DataGridView view)
        {
            view.SuspendLayout();
            view.Columns.Add("ar", "Ankunft");
            view.Columns.Add("dp", "Abfahrt");

            foreach (var sta in path)
            {
                DataGridViewRow stationRow = view.Rows[view.Rows.Add()];

                var ardp = train.GetArrDep(sta);
                var ar = ardp.Arrival.ToShortTimeString();
                var dp = ardp.Departure.ToShortTimeString();
                if (ar != "00:00")
                    stationRow.Cells["ar"].Value = ar;
                if (dp != "00:00")
                    stationRow.Cells["dp"].Value = dp;

                if (path.First() != sta)
                {
                    var cell = stationRow.Cells["ar"];
                    cell.Tag = new Tuple<bool, string>(ardp.TrapeztafelHalt, ardp.Zuglaufmeldung);
                    cell.Style.BackColor = ardp.TrapeztafelHalt ? Color.LightGray : Color.White;
                    if (ardp.Zuglaufmeldung != null && ardp.Zuglaufmeldung != "")
                        cell.Style.Font = fb;
                }
                else
                {
                    stationRow.Cells["ar"].ReadOnly = true;
                    stationRow.Cells["ar"].Style.ApplyStyle(disabledStyle);

                    var cell = stationRow.Cells["dp"];
                    if (ardp.TrapeztafelHalt)
                        throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");
                    cell.Tag = new Tuple<bool, string>(false, ardp.Zuglaufmeldung);
                    if (ardp.Zuglaufmeldung != null && ardp.Zuglaufmeldung != "")
                        cell.Style.Font = fb;
                }

                if (path.Last() == sta)
                {
                    stationRow.Cells["dp"].ReadOnly = true;
                    stationRow.Cells["dp"].Style.ApplyStyle(disabledStyle);
                }

                stationRow.Tag = sta;
                stationRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = sta.SName };
            }

            foreach (DataGridViewColumn column in view.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            view.SelectionChanged += (s, e) =>
            {
                if (view.SelectedCells.Count != 0)
                {
                    var cell = view.SelectedCells[0];
                    trapeztafelToggle.Enabled = cell.ColumnIndex % 2 == 0 && cell.RowIndex != 0;
                    zlmButton.Enabled = cell.RowIndex == 0 ^ cell.ColumnIndex % 2 == 0;

                    var tr = false;
                    if (cell.Tag != null)
                    {
                        var val = (Tuple<bool, string>)cell.Tag;
                        tr = val.Item1;
                    }
                    trapeztafelToggle.Checked = tr;
                }
            };

            view.ResumeLayout();
        }

        private void ValidatingCell(DataGridView view, DataGridViewCellValidatingEventArgs e)
        {
            string val = (string)e.FormattedValue;
            if (val == null || val == "")
                return;

            val = normalizer.Normalize(val);
            if (val != null)
            {
                if (view.EditingControl != null)
                    view.EditingControl.Text = val;
                return;
            }

            MessageBox.Show("Formatierungsfehler: Zeit muss im Format hh:mm, h:mm, h:m, hh:mm, h:, :m, hhmm, hmm oder mm vorliegen!");
            e.Cancel = true;
        }

        private void FormatCell(DataGridViewCellFormattingEventArgs e)
        {
            string val = (string)e.Value;
            if (val == null || val == "")
                return;

            val = normalizer.Normalize(val);
            if (val != null)
            {
                e.Value = val;
                e.FormattingApplied = true;
            }
        }

        private void Trapez(DataGridView view)
        {
            var cells = view.SelectedCells;
            if (cells.Count == 0)
                return;

            var cell = cells[0];
            // Trapeztafelhalt darf nur bei Ankünften sein
            if (cell.ColumnIndex % 2 != 0)
                return;

            var val = (Tuple<bool, string>)cell.Tag;
            var trapez = !(val?.Item1 ?? false);
            cell.Tag = new Tuple<bool, string>(trapez, val?.Item2 ?? "");

            cell.Style.BackColor = trapez ? Color.LightGray : Color.White;
            trapeztafelToggle.Checked = trapez;
        }

        private void Zuglaufmeldung(DataGridView view)
        {
            var cells = view.SelectedCells;
            if (cells.Count == 0)
                return;

            var cell = cells[0];
            // Zuglaufmeldungen dürfen auch bei Abfahrt am ersten Bahnhof sein
            if (cell.RowIndex != 0 && cell.ColumnIndex % 2 != 0)
                return;

            var val = (Tuple<bool, string>)cell.Tag;
            var zlmDialog = new Editor.ZlmEditForm(val?.Item2 ?? "");
            if (zlmDialog.ShowDialog() != DialogResult.OK)
                return;

            cell.Tag = new Tuple<bool, string>(val?.Item1 ?? false, zlmDialog.Zlm);

            cell.Style.Font = zlmDialog.Zlm != "" ? fb : fn;
        }

        private void UpdateTrainDataFromGrid(DataGridView view)
        {
            foreach (DataGridViewRow row in view.Rows)
            {
                Station sta = (Station)row.Tag;

                ArrDep ardp = new ArrDep();

                if (sta != path.First())
                {
                    DataGridViewCell cellAr = row.Cells["ar"];
                    if ((string)cellAr.FormattedValue != "" && cellAr.FormattedValue != null)
                    {
                        TimeSpan tsAr = TimeSpan.Parse((string)cellAr.FormattedValue);
                        ardp.Arrival = tsAr;
                        if (cellAr.Tag != null)
                        {
                            var val = (Tuple<bool, string>)cellAr.Tag;
                            ardp.TrapeztafelHalt = val.Item1;
                            ardp.Zuglaufmeldung = val.Item2;
                        }
                    }
                }

                if (sta != path.Last())
                {
                    DataGridViewCell cellDp = row.Cells["dp"];
                    if ((string)cellDp.FormattedValue != "" && cellDp.FormattedValue != null)
                    {
                        TimeSpan tsDp = TimeSpan.Parse((string)cellDp.FormattedValue);
                        ardp.Departure = tsDp;
                        if (cellDp.Tag != null && sta == path.First())
                        {
                            var val = (Tuple<bool, string>)cellDp.Tag;
                            if (ardp.TrapeztafelHalt)
                                throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");
                            ardp.Zuglaufmeldung = val.Item2;
                        }
                        else if (cellDp.Tag != null)
                            throw new Exception("Keine Abfahrtszelle darf einen Trapeztafelhalt/Zugalufmeldungseintrag enthalten!");
                    }
                }

                train.SetArrDep(sta, ardp);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            UpdateTrainDataFromGrid(dataGridView);

            info.ClearBackup();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }

        #region Events
        private void dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
            => ValidatingCell(dataGridView, e);

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
            => FormatCell(e);

        private void trapeztafelToggle_Click(object sender, EventArgs e)
            => Trapez(dataGridView);

        private void zlmButton_Click(object sender, EventArgs e)
            => Zuglaufmeldung(dataGridView);
        #endregion
    }
}
