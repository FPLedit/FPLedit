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
        private ButtonMenuItem graphItem, showItem, configItem, trainColorItem, stationStyleItem, printItem, exportItem;
        private CheckMenuItem overrideItem;
#pragma warning restore CA2213

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;
            Style.PluginInterface = pluginInterface;

            dpf = new DynamicPreview();
            componentRegistry.Register<IPreviewAction>(dpf);
            pluginInterface.AppClosing += (_, _) => dpf.Close();

            componentRegistry.Register<ISupportsVirtualRoutes>(new SupportsVirtualRoutes());

#if !DEBUG
            if (pluginInterface.Settings.Get<bool>("feature.enable-full-graph-editor"))
            {
#endif
                pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;
                
                graphItem = ((MenuBar)pluginInterface.Menu).CreateItem(T._("B&ildfahrplan"));

                showItem = graphItem.CreateItem(T._("&Anzeigen"), enabled: false, clickHandler: ShowItem_Click);
                graphItem.Items.Add(new SeparatorMenuItem());
                printItem = graphItem.CreateItem(T._("&Drucken"), enabled: false, clickHandler: PrintItem_Click);
                exportItem = graphItem.CreateItem(T._("&Exportieren"), enabled: false, clickHandler: ExportItem_Click);
                graphItem.Items.Add(new SeparatorMenuItem());
                configItem = graphItem.CreateItem(T._("Darste&llung ändern"), enabled: false, clickHandler: (_, _) => ShowForm(new ConfigForm(pluginInterface.Timetable, pluginInterface)));
                trainColorItem = graphItem.CreateItem(T._("&Zugdarstellung ändern"), enabled: false, clickHandler: (_, _) => ShowForm(new TrainStyleForm(pluginInterface)));
                stationStyleItem = graphItem.CreateItem(T._("&Stationsdarstellung ändern"), enabled: false, clickHandler: (_, _) => ShowForm(new StationStyleForm(pluginInterface)));
                overrideItem = graphItem.CreateCheckItem(T._("Verwende nur &Plandarstellung"), isChecked: pluginInterface.Settings.Get<bool>("bifpl.override-entity-styles"),
                    changeHandler: OverrideItem_CheckedChanged);
#if !DEBUG
            }
#endif
        }
        
        private void PrintItem_Click(object sender, EventArgs e)
        {
            try
            {
                new PrintRenderer(pluginInterface).DoPrint();
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error(ex.Message);
            }
        }
        
        private void ExportItem_Click(object sender, EventArgs e)
        {
            try
            {
                new ExportRenderer(pluginInterface).InitPrint();
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error(ex.Message);
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
            printItem.Enabled = exportItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            trainColorItem.Enabled = stationStyleItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
        }

        public void Dispose() => dpf?.Dispose();
    }
}
