using Buchfahrplan.Shared;
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

namespace Buchfahrplan.Standard
{
    public partial class TimetableEditForm : Form
    {
        private IInfo info;

        private const bool TOP_DIRECTION = true;
        private const bool BOTTOM_DIRECTION = false;

        public TimetableEditForm()
        {
            InitializeComponent();
        }

        public void Init(IInfo info)
        {
            this.info = info;
            info.BackupTimetable();

            topFromToLabel.Text = "Züge " + info.Timetable.GetLineName(TOP_DIRECTION);
            bottomFromToLabel.Text = "Züge " + info.Timetable.GetLineName(BOTTOM_DIRECTION);

            InitializeGridView(topDataGridView, TOP_DIRECTION);
            InitializeGridView(bottomDataGridView, BOTTOM_DIRECTION);
        }

        private void InitializeGridView(DataGridView view, bool direction)
        {
            var stations = info.Timetable.GetStationsOrderedByDirection(direction);
            foreach (var sta in stations)
            {
                if (stations.First() != sta)
                    view.Columns.Add(sta.Name + "ar", sta.Name + " an");
                if (stations.Last() != sta)
                    view.Columns.Add(sta.Name + "dp", sta.Name + " ab");
            }

            foreach (var tra in info.Timetable.Trains.Where(t => t.Direction == direction))
            {
                DataGridViewRow trainRow = view.Rows[view.Rows.Add()];

                foreach (var sta in tra.Arrivals.Keys)
                    trainRow.Cells[sta.Name + "ar"].Value = tra.Arrivals[sta].ToShortTimeString();

                foreach (var sta in tra.Departures.Keys)
                    trainRow.Cells[sta.Name + "dp"].Value = tra.Departures[sta].ToShortTimeString();

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.Name };
            }

            foreach (DataGridViewColumn column in view.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private bool UpdateTrainDataFromGrid(Train train, DataGridView view)
        {
            var ar = new Dictionary<Station, TimeSpan>();
            var dp = new Dictionary<Station, TimeSpan>();

            foreach (DataGridViewRow row in view.Rows)
            {
                Train t = (Train)row.Tag;

                if (t != train)
                    continue;

                foreach (var sta in info.Timetable.Stations)
                {
                    if (view.Columns.Contains(sta.Name + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            TimeSpan tsAr = TimeSpan.Parse((string)cellAr.Value);
                            ar.Add(sta, tsAr);
                        }
                    }

                    if (view.Columns.Contains(sta.Name + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                        if ((string)cellDp.Value != "" && cellDp.Value != null)
                        {
                            TimeSpan tsDp = TimeSpan.Parse((string)cellDp.Value);
                            dp.Add(sta, tsDp);
                        }
                    }
                }

                train.Arrivals = ar;
                train.Departures = dp;
                return true;
            }

            return false;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            foreach (var t in info.Timetable.Trains)
                if (!(UpdateTrainDataFromGrid(t, topDataGridView) || UpdateTrainDataFromGrid(t, bottomDataGridView)))
                    throw new Exception("In der Anwendung ist ein interner Fehler aufgetreten!");

            info.ClearBackup();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }
    }
}
