using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System.Linq;

namespace FPLedit.Editor.Trains
{
    internal abstract class BaseTrainsEditor : FDialog<DialogResult>
    {
        private readonly Timetable tt;

        public BaseTrainsEditor(Timetable tt)
        {
            this.tt = tt;
        }

        protected void UpdateListView(GridView view, TrainDirection direction)
        {
            view.DataStore = tt.Trains.Where(t => t.Direction == direction);
        }

        protected void DeleteTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                tt.RemoveTrain((ITrain) view.SelectedItem);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        protected void EditTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                ITrain train = (ITrain) view.SelectedItem;

                if (train is Train tra)
                    using (var tef = new TrainEditForm(tra))
                        if (tef.ShowModal(this) == DialogResult.Ok)
                            UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        protected void NewTrain(GridView view, TrainDirection direction)
        {
            using (var tef = new TrainEditForm(tt, direction))
            {
                if (tef.ShowModal(this) == DialogResult.Ok)
                {
                    tt.AddTrain(tef.Train);
                    if (tef.NextTrain != null)
                        tt.SetTransition(tef.Train, tef.NextTrain);

                    UpdateListView(view, direction);
                }
            }
        }

        protected void CopyTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train) view.SelectedItem;

                using (var tcf = new TrainCopyDialog(train, tt))
                    tcf.ShowModal(this);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }

        protected void SortTrains(GridView view, TrainDirection dir)
        {
            using (var tsd = new TrainSortDialog(dir, tt))
                if (tsd.ShowModal(this) == DialogResult.Ok)
                    UpdateListView(view, dir);
        }
    }
}