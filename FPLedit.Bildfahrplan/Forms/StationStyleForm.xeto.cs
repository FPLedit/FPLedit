using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;

namespace FPLedit.Bildfahrplan.Forms;

internal sealed class StationStyleForm : FDialog<DialogResult>
{
    private readonly IPluginInterface pluginInterface;
    private readonly object backupHandle;

#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = default!;
#pragma warning restore CS0649,CA2213

    public StationStyleForm(IPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        var tt = pluginInterface.Timetable;
        var attrs = new TimetableStyle(tt);
        backupHandle = pluginInterface.BackupTimetable();

        Eto.Serialization.Xaml.XamlReader.Load(this);

        var cc = new ColorCollection(pluginInterface.Settings);
        var ds = new DashStyleHelper();

        var lineWidths = Enumerable.Range(1, 5).Cast<object>().ToArray();

        gridView.AddColumn<StationStyle>(t => t.Station.SName, T._("Name"));
        gridView.AddDropDownColumn<StationStyle>(t => t.HexColor, cc.ColorHexStrings, EtoBindingExtensions.ColorBinding(cc), T._("Farbe"), true);
        gridView.AddDropDownColumn<StationStyle>(t => t.StationWidthInt, lineWidths, Binding.Delegate<int, string>(i => i.ToString()), T._("Linienstärke"), true);
        gridView.AddDropDownColumn<StationStyle>(t => t.LineStyle, ds.Indices.Cast<object>(), Binding.Delegate<int, string>(i => ds.GetDescription(i)), T._("Linientyp"), true);
        gridView.AddCheckColumn<StationStyle>(t => t.Show, T._("Station zeichnen"), true);

        gridView.DataStore = tt.Stations.Select(s => new StationStyle(s, attrs)).ToArray();

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) gridView.AllowEmptySelection = false;

        this.AddCloseHandler();
    }
    private void ResetStationStyle(bool message = true)
    {
        if (gridView.SelectedItem != null)
        {
            ((StationStyle)gridView.SelectedItem).ResetDefaults();
            gridView.ReloadData(gridView.SelectedRow);
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss eine Station ausgewählt werden!"), T._("Stationsdarstellung zurücksetzen"));
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

    private void ResetButton_Click(object sender, EventArgs e)
        => ResetStationStyle();

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Reset = T._("Eintrag &zurücksetzen");
        public static readonly string Title = T._("Stationsdarstellung ändern");
    }
}