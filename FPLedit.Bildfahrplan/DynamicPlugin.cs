using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using Eto.Forms;
using FPLedit.Shared.UI;

namespace FPLedit.Bildfahrplan
{
    [Plugin("Dynamische Bildfahrplan-Vorschau", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class DynamicPlugin : IPlugin, IDisposable
    {
        private IPluginInterface pluginInterface;
        private DynamicPreview dpf;
        
        private ButtonMenuItem showItem, configItem, trainColorItem, stationStyleItem, printItem;
        private CheckMenuItem overrideItem;
        
        public void Init(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Style.pluginInterface = pluginInterface;

            dpf = new DynamicPreview();
            pluginInterface.Register<IPreviewAction>(dpf);
            pluginInterface.AppClosing += (s, e) => dpf.Close();

            if (pluginInterface.Settings.Get<bool>("feature.enable-full-graph-editor"))
            {
                pluginInterface.Register<IExport>(new BitmapExport());
                
                pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
                
                var graphItem = ((MenuBar)pluginInterface.Menu).CreateItem("B&ildfahrplan");

                showItem = graphItem.CreateItem("&Anzeigen", enabled: false, clickHandler: ShowItem_Click);
                configItem = graphItem.CreateItem("Darste&llung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new ConfigForm(pluginInterface.Timetable, pluginInterface.Settings)));
                printItem = graphItem.CreateItem("&Drucken", enabled: false, clickHandler: PrintItem_Click);
                trainColorItem = graphItem.CreateItem("&Zugdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new TrainStyleForm(pluginInterface)));
                stationStyleItem = graphItem.CreateItem("&Stationsdarstellung ändern", enabled: false, clickHandler: (s, ev) => ShowForm(new StationStyleForm(pluginInterface)));
                overrideItem = graphItem.CreateCheckItem("Verwende nur &Plandarstellung", isChecked: pluginInterface.Settings.Get<bool>("bifpl.override-entity-styles"),
                    changeHandler: OverrideItem_CheckedChanged);
            }
        }
        
        private void PrintItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("WARNUNG! Die Folgende Funktion ist experimentell und ungetest. Fortfahren?", "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning) == DialogResult.Yes)
            {
                var route = (pluginInterface.Timetable.Type == TimetableType.Network) ? pluginInterface.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
                using (var pr = new PrintRenderer(pluginInterface, route))
                    pr.InitPrint();
            }
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
                printItem.Text = "Drucken (aktuelle Route)";
            else
                printItem.Text = "Drucken";
        }

        public void Dispose() => dpf?.Dispose();
    }
}
