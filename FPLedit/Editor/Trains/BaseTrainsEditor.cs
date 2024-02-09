using System;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System.Linq;

namespace FPLedit.Editor.Trains;

internal abstract class BaseTrainsEditor : FDialog<DialogResult>
{
    private readonly Timetable tt;
    private readonly IPluginInterface pluginInterface;

    protected BaseTrainsEditor(IPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        tt = pluginInterface.Timetable;
    }

    protected void UpdateListView(GridView view, TrainDirection direction)
    {
        view.DataStore = tt.Trains.Where(t => t.Direction == direction).ToArray();
    }

    protected void DeleteTrain(GridView view, TrainDirection dir, bool message = true)
    {
        var msgCaption = T._nc("BaseTrainsEditor", "Zug löschen", "Züge löschen", view.SelectedItems.Count());

        if (view.SelectedItems.Any())
        {
            var trains = view.SelectedItems.OfType<Train>().ToArray();
            if (trains.Length == view.SelectedItems.Count())
            {
                if (trains.Any(t => t.TrainLinks.Any(l => l.TrainCount > 0)))
                {
                    if (message)
                        MessageBox.Show(T._("Der Zug kann nicht gelöscht werden, da er mindestens von einem verlinkten Zug referenziert wird!"), msgCaption);
                }
                else if (MessageBox.Show(T._n("Zug wirklich löschen?", "Züge wirklich löschen?", trains.Length), msgCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)

                {
                    foreach (var t in trains)
                        tt.RemoveTrain(t);
                    UpdateListView(view, dir);
                }
            }
            else if (message)
                MessageBox.Show(T._("Verlinke Züge können nicht gelöscht werden."), msgCaption);
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss mindestens ein Zug ausgewählt werden!"), msgCaption);
    }

    protected void EditTrain(GridView view, TrainDirection dir, bool message = true)
    {
        var msgCaption = T._("Zug bearbeiten");

        if (view.SelectedItem != null)
        {
            if (view.SelectedItem is Train tra)
            {
                try
                {
                    using var tef = new TrainEditForm(tra);
                    if (tef.ShowModal(this) != null)
                        UpdateListView(view, dir);
                }
                catch (Exception e)
                {
                    pluginInterface.Logger.LogException(e);
                    MessageBox.Show(T._("Korrupter Zug konnte nicht geladen werden. Mehr Informationen im Log: {0}", e.Message), msgCaption);
                }
            }
            else if (message)
                MessageBox.Show(T._("Verlinke Züge können nicht bearbeitet werden."), msgCaption);
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss ein Zug ausgewählt werden!"), msgCaption);
    }

    protected void NewTrain(GridView view, TrainDirection direction)
    {
        using var tef = new TrainEditForm(tt, direction);
        var result = tef.ShowModal(this);
        if (result != null)
        {
            tt.AddTrain(result.Train);
            if (result.NextTrains.Any())
                tt.SetTransitions(result.Train, result.NextTrains);

            UpdateListView(view, direction);
        }
    }

    protected void CopyTrain(GridView view, TrainDirection dir, bool message = true)
    {
        var msgCaption = T._nc("BaseTrainsEditor", "Zug kopieren", "Züge kopieren", view.SelectedItems.Count());

        if (view.SelectedItems.Any())
        {
            var trains = view.SelectedItems.OfType<Train>().ToArray();
            if (trains.Length == view.SelectedItems.Count())
            {
                using (var tcf = new TrainCopyDialog(trains, tt))
                    tcf.ShowModal(this);

                UpdateListView(view, dir);
            }
            else if (message)
                MessageBox.Show(T._("Verlinke Züge können nicht kopiert werden."), msgCaption);
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss mindestens ein Zug ausgewählt werden!"), msgCaption);
    }

    protected void SortTrains(GridView view, TrainDirection dir)
    {
        using var tsd = new TrainSortDialog(dir, tt);
        if (tsd.ShowModal(this) == DialogResult.Ok)
            UpdateListView(view, dir);
    }
}