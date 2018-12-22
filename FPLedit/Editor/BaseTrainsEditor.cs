using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class BaseTrainsEditor : Dialog<DialogResult>
    {
        private Timetable tt;
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
                tt.RemoveTrain((Train)view.SelectedItem);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug löschen");
        }

        protected void EditTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                Train train = (Train)view.SelectedItem;

                TrainEditForm tef = new TrainEditForm(train);
                if (tef.ShowModal(this) == DialogResult.Ok)
                    UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug bearbeiten");
        }

        protected void NewTrain(GridView view, TrainDirection direction)
        {
            TrainEditForm tef = new TrainEditForm(tt, direction);
            if (tef.ShowModal(this) == DialogResult.Ok)
            {
                tt.AddTrain(tef.Train);

                UpdateListView(view, direction);
            }
        }

        protected void CopyTrain(GridView view, TrainDirection dir, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var train = (Train)view.SelectedItem;

                var tcf = new TrainCopyDialog(train, tt);
                tcf.ShowModal(this);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zug kopieren");
        }
    }
}
