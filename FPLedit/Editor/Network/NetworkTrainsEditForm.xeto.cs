using Eto.Forms;
using FPLedit.Editor.Trains;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Shared.UI.Network;

namespace FPLedit.Editor.Network;

internal sealed class NetworkTrainsEditForm : BaseTrainsEditor
{
    private readonly IPluginInterface pluginInterface;
    private readonly Timetable tt;
    private readonly object backupHandle;

#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = default!;
    private readonly Button editPathButton = default!, editButton = default!, deleteButton = default!, copyButton = default!;
#pragma warning restore CS0649,CA2213

    public NetworkTrainsEditForm(IPluginInterface pluginInterface) : base(pluginInterface.Timetable)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        this.pluginInterface = pluginInterface;
        tt = pluginInterface.Timetable;
        backupHandle = pluginInterface.BackupTimetable();

        // TRANSLATORS: "L" ist kurz für "Link"
        gridView.AddFuncColumn<ITrain>(t => t.IsLink ? T._("L") : "", "");
        gridView.AddFuncColumn<ITrain>(t => t.TName, T._("Zugnummer"));
        gridView.AddFuncColumn<ITrain>(t => t.Locomotive, T._("Tfz"));
        gridView.AddFuncColumn<ITrain>(t => t.Mbr, T._("Mbr"));
        gridView.AddFuncColumn<ITrain>(t => t.Last, T._("Last"));
        gridView.AddFuncColumn<ITrain>(t => t.Days.DaysToString(false), T._("Verkehrstage"));
        gridView.AddFuncColumn<ITrain>(BuildPath, T._("Laufweg"));
        gridView.AddFuncColumn<ITrain>(t => t.Comment, T._("Kommentar"));

        gridView.MouseDoubleClick += (_, _) => EditTrain(gridView, TrainDirection.tr, false);

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) gridView.AllowEmptySelection = false;
        gridView.AllowMultipleSelection = true;

        UpdateListView(gridView, TrainDirection.tr);

        if (Eto.Platform.Instance.IsWpf)
            KeyDown += HandleKeystroke;
        else
            gridView.KeyDown += HandleKeystroke;

        gridView.SelectedItemsChanged += GridViewOnSelectedItemsChanged;

        this.AddCloseHandler();
        this.AddSizeStateHandler();

        // Bugfix, Window closes on enter [Enter]
        // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        if (!Platform.IsWpf)
            DefaultButton = null;
    }

    private void GridViewOnSelectedItemsChanged(object? sender, EventArgs e)
    {
        editButton.Enabled = editPathButton.Enabled
            = gridView.SelectedItem != null && !((ITrain) gridView.SelectedItem).IsLink;
        deleteButton.Enabled = copyButton.Enabled
            = gridView.SelectedItems.Any() && gridView.SelectedItems.All(t => !(t as ITrain)!.IsLink);
    }

    private void HandleKeystroke(object? sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Delete)
            DeleteTrain(gridView, TrainDirection.tr, false);
        else if (e is { Key: Keys.C, Control: true })
            CopyTrain(gridView, TrainDirection.tr, false);
        else if (e is { Key: Keys.P, Control: true })
            EditPath(gridView, false);
        else if (e is { Key: Keys.B, Control: true } || (e.Key == Keys.Enter))
            EditTrain(gridView, TrainDirection.tr, false);
        else if (e is { Key: Keys.N, Control: true })
            NewTrain(gridView);
    }

    private string BuildPath(ITrain t)
    {
        var path = t.GetPath();
        return path.FirstOrDefault()?.SName + " - " + path.LastOrDefault()?.SName;
    }

    private void EditPath(GridView view, bool message = true)
    {
        if (view.SelectedItem != null)
        {
            if (view.SelectedItem is Train train)
            {
                using var tpf = TrainPathForm.EditPath(pluginInterface, train);
                if (tpf.ShowModal(this) != null)
                    UpdateListView(view, TrainDirection.tr);
            }
            else if (message)
                MessageBox.Show(T._("Verlinke Züge können nicht bearbeitet werden."), T._("Laufweg bearbeiten"));
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss ein Zug ausgewählt werden!"), T._("Laufweg bearbeiten"));
    }

    private void NewTrain(GridView view)
    {
        using var tpf = TrainPathForm.NewTrain(pluginInterface);
        var pathResult = tpf.ShowModal(this);
        if (pathResult == null)
            return;

        using var tef = new TrainEditForm(pluginInterface.Timetable, TrainDirection.tr, pathResult.Path);
        var result = tef.ShowModal(this);
        if (result != null)
        {
            tt.AddTrain(result.Train);
            if (result.NextTrains.Any())
                tt.SetTransitions(result.Train, result.NextTrains);

            UpdateListView(view, TrainDirection.tr);
        }
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        pluginInterface.ClearBackup(backupHandle);
        Result = DialogResult.Ok;
        this.NClose();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        Result = DialogResult.Cancel;
        pluginInterface.RestoreTimetable(backupHandle);
        this.NClose();
    }

    private void NewButton_Click(object sender, EventArgs e)
        => NewTrain(gridView);

    private void EditButton_Click(object sender, EventArgs e)
        => EditTrain(gridView, TrainDirection.tr);

    private void DeleteButton_Click(object sender, EventArgs e)
        => DeleteTrain(gridView, TrainDirection.tr);

    private void CopyButton_Click(object sender, EventArgs e)
        => CopyTrain(gridView, TrainDirection.tr);

    private void EditPathButton_Click(object sender, EventArgs e)
        => EditPath(gridView);

    private void SortButton_Click(object sender, EventArgs e)
        => SortTrains(gridView, TrainDirection.tr);

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Züge bearbeiten");
        public static readonly string Sort = T._("Züge sortieren");
        public static readonly string Copy = T._("Zug kopieren/verschieben");
        public static readonly string Delete = T._("Zug löschen");
        public static readonly string Edit = T._("Zug bearbeiten");
        public static readonly string New = T._("Neuer Zug");
        public static readonly string EditPath = T._("Laufweg bearbeiten");
    }
}