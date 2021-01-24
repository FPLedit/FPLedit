using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using Eto.Forms;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared.UI;

namespace FPLedit.Bildfahrplan
{
    [Plugin("Dynamische Bildfahrplan-Vorschau", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class DynamicPlugin : IPlugin, IDisposable
    {
        private IPluginInterface pluginInterface;
        private DynamicPreview dpf;
        
#pragma warning disable CA2213
        private ButtonMenuItem showItem, configItem, trainColorItem, stationStyleItem, printItem;
        private CheckMenuItem overrideItem;
#pragma warning restore CA2213
        
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            Style.PluginInterface = pluginInterface;

            if (!GdiAvailabilityTest.Test())
            {
                pluginInterface.Logger.Error(T._("Die Bibliothek libgdiplus wurde nicht gefunden! Die Bildfahrplanfunktionen stehen nicht zur Verfügung. Zur Installation siehe Installatiosnhinweise unter https://fahrplan.manuelhu.de/download/."));
                return;
            }

            dpf = new DynamicPreview();
            componentRegistry.Register<IPreviewAction>(dpf);
            pluginInterface.AppClosing += (s, e) => dpf.Close();

#if !DEBUG
            if (pluginInterface.Settings.Get<bool>("feature.enable-full-graph-editor"))
            {
#endif
                componentRegistry.Register<IExport>(new BitmapExport());
                
                pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
                
#pragma warning disable CA2000                
                var graphItem = ((MenuBar)pluginInterface.Menu).CreateItem(T._("B&ildfahrplan"));
#pragma warning restore CA2000

                showItem = graphItem.CreateItem(T._("&Anzeigen"), enabled: false, clickHandler: ShowItem_Click);
                configItem = graphItem.CreateItem(T._("Darste&llung ändern"), enabled: false, clickHandler: (s, ev) => ShowForm(new ConfigForm(pluginInterface.Timetable, pluginInterface)));
                printItem = graphItem.CreateItem(T._("&Drucken"), enabled: false, clickHandler: PrintItem_Click);
                trainColorItem = graphItem.CreateItem(T._("&Zugdarstellung ändern"), enabled: false, clickHandler: (s, ev) => ShowForm(new TrainStyleForm(pluginInterface)));
                stationStyleItem = graphItem.CreateItem(T._("&Stationsdarstellung ändern"), enabled: false, clickHandler: (s, ev) => ShowForm(new StationStyleForm(pluginInterface)));
                overrideItem = graphItem.CreateCheckItem(T._("Verwende nur &Plandarstellung"), isChecked: pluginInterface.Settings.Get<bool>("bifpl.override-entity-styles"),
                    changeHandler: OverrideItem_CheckedChanged);
#if !DEBUG
            }
#endif
        }
        
        private void PrintItem_Click(object sender, EventArgs e)
        {
#if !DEBUG
            if (MessageBox.Show(T._("WARNUNG! Die folgende Funktion ist experimentell und ungetest. Fortfahren?"), "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning) == DialogResult.Yes)
            {
#endif
                var route = (pluginInterface.Timetable.Type == TimetableType.Network) ? pluginInterface.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
                using (var pr = new PrintRenderer(pluginInterface, route))
                    pr.InitPrint();
#if !DEBUG
            }
#endif
        }

        private void ShowItem_Click(object sender, EventArgs e) => dpf.Show(pluginInterface);

        private void ShowForm(Dialog<DialogResult> form)
        {
            pluginInterface.StageUndoStep();
            if (form.ShowModal((Window)pluginInterface.RootForm) == DialogResult.Ok)
                pluginInterface.SetUnsaved();
            if (!form.IsDisposed)
                form.Dispose();
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
                printItem.Text = T._("Drucken (aktuelle Route)");
            else
                printItem.Text = T._("Drucken");
        }

        public void Dispose() => dpf?.Dispose();
    }
}
