﻿using Eto.Forms;
using FPLedit.Editor.Trains;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;

namespace FPLedit.Editor.Linear;

internal sealed class LinearTrainsEditForm : BaseTrainsEditor
{
    private readonly IPluginInterface pluginInterface;
    private readonly object backupHandle;

#pragma warning disable CS0649,CA2213
    private readonly GridView topGridView = default!, bottomGridView = default!;
    private readonly Label topLineLabel = default!, bottomLineLabel = default!;
    private readonly Button topEditButton = default!, topDeleteButton = default!, topCopyButton = default!, bottomEditButton = default!, bottomDeleteButton = default!, bottomCopyButton = default!;
#pragma warning restore CS0649,CA2213

    private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
    private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

    private GridView? active;

    public LinearTrainsEditForm(IPluginInterface pluginInterface) : base(pluginInterface.Timetable)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);
        this.pluginInterface = pluginInterface;
        var tt = pluginInterface.Timetable;
        backupHandle = pluginInterface.BackupTimetable();

        InitListView(topGridView, new []{ topEditButton }, new []{ topDeleteButton, topCopyButton });
        InitListView(bottomGridView, new []{ bottomEditButton }, new []{ bottomDeleteButton, bottomCopyButton });

        var rt = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);
        topLineLabel.Text = T._("Züge {0}", rt.GetRouteName(TOP_DIRECTION.IsSortReverse()));
        bottomLineLabel.Text = T._("Züge {0}", rt.GetRouteName(BOTTOM_DIRECTION.IsSortReverse()));
        UpdateListView(topGridView, TOP_DIRECTION);
        UpdateListView(bottomGridView, BOTTOM_DIRECTION);

        bottomGridView.MouseDoubleClick += (_, _) => EditTrain(bottomGridView, BOTTOM_DIRECTION, false);
        topGridView.MouseDoubleClick += (_, _) => EditTrain(topGridView, TOP_DIRECTION, false);

        if (Eto.Platform.Instance.IsWpf)
            KeyDown += HandleKeystroke;

        this.AddCloseHandler();
        this.AddSizeStateHandler();

        // Bugfix, Window closes on enter [Enter]
        // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        if (!Platform.IsWpf)
            DefaultButton = null;
    }

    private void HandleKeystroke(object? sender, KeyEventArgs e)
    {
        if (active == null)
            return;
        var dir = active == topGridView ? TOP_DIRECTION : BOTTOM_DIRECTION;

        if (e.Key == Keys.Delete)
            DeleteTrain(active, dir, false);
        else if (e is { Key: Keys.B, Control: true } || (e.Key == Keys.Enter))
            EditTrain(active, dir, false);
        else if (e is { Key: Keys.N, Control: true })
            NewTrain(active, dir);
        else if (e is { Key: Keys.C, Control: true })
            CopyTrain(active, dir);
    }

    private void InitListView(GridView view, Button[] singleButtons, Button[] multiButtons)
    {
        // TRANSLATORS: "L" ist kurz für "Link"
        view.AddFuncColumn<ITrain>(t => t.IsLink ? T._("L") : "", "");
        view.AddFuncColumn<ITrain>(t => t.TName, T._("Zugnummer"));
        view.AddFuncColumn<ITrain>(t => t.Locomotive, T._("Tfz"));
        view.AddFuncColumn<ITrain>(t => t.Mbr, T._("Mbr"));
        view.AddFuncColumn<ITrain>(t => t.Last, T._("Last"));
        view.AddFuncColumn<ITrain>(t => t.Days.DaysToString(), T._("Verkehrstage"));
        view.AddFuncColumn<ITrain>(t => t.Comment, T._("Kommentar"));

        view.GotFocus += (_, _) => active = view;

        if (!Eto.Platform.Instance.IsWpf)
            view.KeyDown += HandleKeystroke;

        view.SelectedItemsChanged += (_, _) =>
        {
            foreach (var button in singleButtons)
                button.Enabled = view.SelectedItem != null && !((ITrain) view.SelectedItem).IsLink;
            foreach (var button in multiButtons)
                button.Enabled = view.SelectedItems.Any() && view.SelectedItems.All(t => !(t as ITrain)!.IsLink);
        };

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) view.AllowEmptySelection = false;
        view.AllowMultipleSelection = true;
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

    #region Events
    private void TopNewButton_Click(object sender, EventArgs e)
        => NewTrain(topGridView, TOP_DIRECTION);

    private void TopEditButton_Click(object sender, EventArgs e)
        => EditTrain(topGridView, TOP_DIRECTION);

    private void TopDeleteButton_Click(object sender, EventArgs e)
        => DeleteTrain(topGridView, TOP_DIRECTION);

    private void TopSortButton_Click(object sender, EventArgs e)
        => SortTrains(topGridView, TOP_DIRECTION);

    private void BottomNewButton_Click(object sender, EventArgs e)
        => NewTrain(bottomGridView, BOTTOM_DIRECTION);

    private void BottomEditButton_Click(object sender, EventArgs e)
        => EditTrain(bottomGridView, BOTTOM_DIRECTION);

    private void BottomDeleteButton_Click(object sender, EventArgs e)
        => DeleteTrain(bottomGridView, BOTTOM_DIRECTION);

    private void TopCopyButton_Click(object sender, EventArgs e)
        => CopyTrain(topGridView, TOP_DIRECTION, true);

    private void BottomCopyButton_Click(object sender, EventArgs e)
        => CopyTrain(bottomGridView, BOTTOM_DIRECTION, true);

    private void BottomSortButton_Click(object sender, EventArgs e)
        => SortTrains(bottomGridView, BOTTOM_DIRECTION);
    #endregion
        
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
    }
}