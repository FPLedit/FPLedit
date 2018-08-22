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
    [Plugin("Modul für Bildfahrpläne", Pvi.From, Pvi.UpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ButtonMenuItem showItem, configItem, trainColorItem, stationStyleItem, printItem;
        private CheckMenuItem overrideItem;

        private TimetableStyle attrs;

        public void Init(IInfo info)
        {
            this.info = info;
            Style.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new BitmapExport());

            ButtonMenuItem item = ((MenuBar)info.Menu).CreateItem("Bildfahrplan");

            showItem = item.CreateItem("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            configItem = item.CreateItem("Darstellung ändern");
            configItem.Enabled = false;
            configItem.Click += ConfigItem_Click;

            printItem = item.CreateItem("Drucken");
            printItem.Enabled = false;
            printItem.Click += PrintItem_Click;

            trainColorItem = item.CreateItem("Zugdarstellung ändern");
            trainColorItem.Enabled = false;
            trainColorItem.Click += TrainColorItem_Click;

            stationStyleItem = item.CreateItem("Stationsdarstellung ändern");
            stationStyleItem.Enabled = false;
            stationStyleItem.Click += StationStyleItem_Click;

            overrideItem = item.CreateCheckItem("Verwende nur Plandarstellung");
            overrideItem.CheckedChanged += OverrideItem_CheckedChanged;
            overrideItem.Checked = info.Settings.Get<bool>("bifpl.override-entity-styles");
        }

        private void OverrideItem_CheckedChanged(object sender, EventArgs e)
        {
            info.Settings.Set("bifpl.override-entity-styles", overrideItem.Checked.ToString().ToLower());
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            configItem.Enabled = e.FileState.Opened;
            printItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            trainColorItem.Enabled = stationStyleItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;

            attrs = new TimetableStyle(info.Timetable);

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

        private void TrainColorItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            TrainColorForm tcf = new TrainColorForm(info);
            if (tcf.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
        }

        private void StationStyleItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            var scf = new StationStyleForm(info);
            if (scf.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
        }

        private void ConfigItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            ConfigForm cnf = new ConfigForm(info.Timetable, info.Settings);
            if (cnf.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
        }

        private void PrintItem_Click(object sender, EventArgs e)
        {
            var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            new PrintRenderer(info.Timetable, route).InitPrint();
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            new PreviewForm(info, route).ShowModal(info.RootForm);
        }
    }
}
