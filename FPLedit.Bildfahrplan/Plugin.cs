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
        private IInfo info;
        private ButtonMenuItem showItem, configItem, trainColorItem, stationStyleItem, printItem;
        private CheckMenuItem overrideItem;

        public void Init(IInfo info)
        {
            this.info = info;
            Style.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new BitmapExport());

            var graphItem = ((MenuBar)info.Menu).CreateItem("Bildfahrplan");

            showItem = graphItem.CreateItem("Anzeigen", enabled: false, clickHandler: ShowItem_Click);
            configItem = graphItem.CreateItem("Darstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new ConfigForm(info.Timetable, info.Settings)));
            printItem = graphItem.CreateItem("Drucken", enabled: false, clickHandler: PrintItem_Click);
            trainColorItem = graphItem.CreateItem("Zugdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new TrainColorForm(info)));
            stationStyleItem = graphItem.CreateItem("Stationsdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new StationStyleForm(info)));
            overrideItem = graphItem.CreateCheckItem("Verwende nur Plandarstellung", isChecked: info.Settings.Get<bool>("bifpl.override-entity-styles"),
                changeHandler: OverrideItem_CheckedChanged);
        }

        private void OverrideItem_CheckedChanged(object sender, EventArgs e)
        {
            info.Settings.Set("bifpl.override-entity-styles", overrideItem.Checked);
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            configItem.Enabled = e.FileState.Opened;
            printItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            trainColorItem.Enabled = stationStyleItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;

            if (e.FileState.Opened && info.Timetable.Type == TimetableType.Network)
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
            var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            using (var pr = new PrintRenderer(info, route))
                pr.InitPrint();
        }

        private void ShowItem_Click(object sender, EventArgs e) => new PreviewForm(info).Show(); // no using intended!

        private void ShowForm(Dialog<DialogResult> form)
        {
            info.StageUndoStep();
            if (form.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
            form.Dispose();
        }
    }
#endif
}
