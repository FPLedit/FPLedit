using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;

namespace FPLedit.Bildfahrplan.Forms;

internal sealed class TrainStyleForm : FDialog<DialogResult>
{
    private readonly IPluginInterface pluginInterface;
    private readonly object backupHandle;

#pragma warning disable CS0649,CA2213
    private readonly GridView gridView = default!;
#pragma warning restore CS0649,CA2213

    public TrainStyleForm(IPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        var tt = pluginInterface.Timetable;
        var attrs = new TimetableStyle(tt);
        backupHandle = pluginInterface.BackupTimetable();

        Eto.Serialization.Xaml.XamlReader.Load(this);

        var cc = new ColorCollection(pluginInterface.Settings);
        var ds = new DashStyleHelper();

        var lineWidths = Enumerable.Range(1, 5).Cast<object>().ToArray();

        gridView.AddColumn<TrainStyle>(t => t.Train.TName, T._("Zugnummer"));
        gridView.AddDropDownColumn<TrainStyle>(t => t.HexColor, cc.ColorHexStrings, EtoBindingExtensions.ColorBinding(cc), T._("Farbe"), true);
        gridView.AddDropDownColumn<TrainStyle>(t => t.TrainWidthInt, lineWidths, Binding.Delegate<int, string>(i => i.ToString()), T._("Linienstärke"), true);
        gridView.AddDropDownColumn<TrainStyle>(t => t.LineStyle, ds.Indices.Cast<object>(), Binding.Delegate<int, string>(i => ds.GetDescription(i)), T._("Linientyp"), true);
        gridView.AddCheckColumn<TrainStyle>(t => t.Show, T._("Zug zeichnen"), true);

        gridView.DataStore = tt.Trains.OfType<IWritableTrain>().Select(t => new TrainStyle(t, attrs)).ToArray();

        // This allows the selection of the last row on Wpf, see Eto#2443.
        if (Platform.IsGtk) gridView.AllowEmptySelection = false;

        this.AddCloseHandler();
    }

    private void ResetTrainStyle(bool message = true)
    {
        if (gridView.SelectedItem != null)
        {
            ((TrainStyle)gridView.SelectedItem).ResetDefaults();
            gridView.ReloadData(gridView.SelectedRow);
        }
        else if (message)
            MessageBox.Show(T._("Zuerst muss ein Zug ausgewählt werden!"), T._("Zugdarstellung zurücksetzen"));
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
        => ResetTrainStyle();

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Reset = T._("Eintrag &zurücksetzen");
        public static readonly string Title = T._("Zugdarstellung ändern");
    }
}