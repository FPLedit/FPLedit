using Eto.Forms;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Buchfahrplan.Forms;

internal sealed class VelocityForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = null!;
    private readonly Button deleteButton = null!;
#pragma warning restore CS0649,CA2213

    private readonly IPluginInterface pluginInterface = null!;
    private readonly Route route = null!;
    private readonly Timetable tt = null!;
    private readonly BfplAttrs? attrs;
    private readonly object backupHandle = null!;

    private VelocityForm()
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        gridView.AddFuncColumn<IStation>(s => s.Positions.GetPosition(route!.Index).ToString()!, T._("km"));
        gridView.AddFuncColumn<IStation>(s => s.SName, T._("Name"));
        gridView.AddFuncColumn<IStation>(s => s.Vmax.GetValue(route!.Index)!, T._("Vmax"));
        gridView.AddFuncColumn<IStation>(s => s.Wellenlinien.GetValue(route!.Index).ToString(), T._("Wellenlinien"));

        gridView.MouseDoubleClick += (_, _) => EditPoint(false);

        gridView.SelectedItemsChanged += (_, _) => SelectPoint();

        this.AddCloseHandler();
        this.AddSizeStateHandler();
    }

    public VelocityForm(IPluginInterface pluginInterface, Route route) : this()
    {
        this.pluginInterface = pluginInterface;
        tt = pluginInterface.Timetable;
        this.route = route;

        attrs = BfplAttrs.GetAttrs(tt) ?? BfplAttrs.CreateAttrs(tt);

        backupHandle = pluginInterface.BackupTimetable();
        UpdateListView();
    }

    private void UpdateListView()
    {
        var points = new List<IStation>();
        points.AddRange(route.Stations);
        if (attrs != null)
            points.AddRange(attrs.GetRoutePoints(route.Index));

        gridView.DataStore = points.OrderBy(o => o.Positions.GetPosition(route.Index)).ToArray();
    }

    private void AddPoint()
    {
        using var vef = new VelocityEditForm(tt, route.Index);
        if (vef.ShowModal(this) == DialogResult.Ok)
        {
            var p = (BfplPoint) vef.Station;

            var pos = p.Positions.GetPosition(route.Index);
            if (pos < route.MinPosition || pos > route.MaxPosition)
            {
                MessageBox.Show(T._("Die Position muss im Streckenbereich liegen, also zwischen {0} und {1}!", route.MinPosition, route.MaxPosition), "FPLedit");
                return;
            }

            attrs?.AddPoint(p);
            UpdateListView();
        }
    }

    private void EditPoint(bool message = true)
    {
        if (gridView.SelectedItem != null)
        {
            var sta = (IStation) gridView.SelectedItem;

            using var vef = new VelocityEditForm(sta, route.Index);
            if (vef.ShowModal(this) == DialogResult.Ok)
                UpdateListView();
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss eine Zeile ausgewählt werden!"), T._("Höchstgeschwindigkeit ändern"));
    }

    private void RemovePoint(bool message = true)
    {
        if (gridView.SelectedItem != null)
        {
            var sta = gridView.SelectedItem;

            if (sta is Station)
                throw new InvalidOperationException("Invalid state: Stations cannot be deleted with this function.");
            if (sta is BfplPoint point)
            {
                attrs?.RemovePoint(point);
                UpdateListView();
            }
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss eine Zeile ausgewählt werden!"), T._("Löschen"));
    }

    private void SelectPoint()
    {
        deleteButton.Enabled = (gridView.SelectedItem is BfplPoint);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        Result = DialogResult.Cancel;
        pluginInterface.RestoreTimetable(backupHandle);
        this.NClose();
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        Result = DialogResult.Ok;
        pluginInterface.ClearBackup(backupHandle);

        this.NClose();
    }

    #region Events

    private void EditButton_Click(object sender, EventArgs e)
        => EditPoint();

    private void AddButton_Click(object sender, EventArgs e)
        => AddPoint();

    private void DeleteButton_Click(object sender, EventArgs e)
        => RemovePoint();

    #endregion
        
    public static class L
    {
        public static readonly string Title = T._("Höchstgeschwindigkeiten ändern");
        public static readonly string Add = T._("Wechsel &hizufügen");
        public static readonly string Edit = T._("Eintrag &bearbeiten");
        public static readonly string Delete = T._("Wechsel &löschen");
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
    }
}