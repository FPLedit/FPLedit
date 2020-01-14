using Eto.Forms;
using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan
{
#if !DISABLE_EDIT_BIFPL
    [Plugin("Modul für Bildfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IPluginInterface pluginInterface;
        private ButtonMenuItem showItem, configItem, trainColorItem, stationStyleItem, printItem;
        private CheckMenuItem overrideItem;

        public void Init(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Style.pluginInterface = pluginInterface;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            pluginInterface.Register<IExport>(new BitmapExport());

            var graphItem = ((MenuBar)pluginInterface.Menu).CreateItem("Bildfahrplan");

            showItem = graphItem.CreateItem("Anzeigen", enabled: false, clickHandler: ShowItem_Click);
            configItem = graphItem.CreateItem("Darstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new ConfigForm(pluginInterface.Timetable, pluginInterface.Settings)));
            printItem = graphItem.CreateItem("Drucken", enabled: false, clickHandler: PrintItem_Click);
            trainColorItem = graphItem.CreateItem("Zugdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new TrainColorForm(pluginInterface)));
            stationStyleItem = graphItem.CreateItem("Stationsdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new StationStyleForm(pluginInterface)));
            overrideItem = graphItem.CreateCheckItem("Verwende nur Plandarstellung", isChecked: pluginInterface.Settings.Get<bool>("bifpl.override-entity-styles"),
                changeHandler: OverrideItem_CheckedChanged);
        }

        private void OverrideItem_CheckedChanged(object sender, EventArgs e)
        {
            pluginInterface.Settings.Set("bifpl.override-entity-styles", overrideItem.Checked);
        }

        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            configItem.Enabled = e.FileState.Opened;
            printItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            trainColorItem.Enabled = stationStyleItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;

            if (e.FileState.Opened && pluginInterface.Timetable.Type == TimetableType.Network)
            {
                showItem.Text = "Anzeigen (aktuelle Route)";
                printItem.Text = "Drucken (aktuelle Route)";
            }
            else
            {
                showItem.Text = "Anzeigen";
                printItem.Text = "Drucken";
            }
        }

        private void PrintItem_Click(object sender, EventArgs e)
        {
            var route = (pluginInterface.Timetable.Type == TimetableType.Network) ? pluginInterface.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            using (var pr = new PrintRenderer(pluginInterface, route))
                pr.InitPrint();
        }

        private void ShowItem_Click(object sender, EventArgs e) => new PreviewForm(pluginInterface).Show(); // no using intended!

        private void ShowForm(Dialog<DialogResult> form)
        {
            pluginInterface.StageUndoStep();
            if (form.ShowModal(pluginInterface.RootForm) == DialogResult.Ok)
                pluginInterface.SetUnsaved();
            form.Dispose();
        }
    }
#endif
}
