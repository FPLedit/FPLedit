﻿using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using Eto.Forms;
using FPLedit.Shared.UI;

namespace FPLedit.Bildfahrplan;

[Plugin("Dynamische Bildfahrplan-Vorschau", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
public sealed class DynamicPlugin : IPlugin, IDisposable
{
    private IPluginInterface pluginInterface = default!;
    private DynamicPreview? dpf;

    private const string OVERRIDE_SETTINGS_KEY = "bifpl.override-entity-styles";

#pragma warning disable CA2213
    private ButtonMenuItem graphItem = default!, showItem = default!, configItem = default!, trainColorItem = default!, stationStyleItem = default!, printItem = default!, exportItem = default!;
    private CheckMenuItem overrideItem = default!;
#pragma warning restore CA2213

    public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        this.pluginInterface = pluginInterface;
        Style.OverrideEntityStyle = pluginInterface.Settings.Get<bool>(OVERRIDE_SETTINGS_KEY);

        dpf = new DynamicPreview();
        componentRegistry.Register<IPreviewAction>(dpf);
        pluginInterface.AppClosing += (_, _) => dpf.Close();

        componentRegistry.Register<ISupportsVirtualRoutes>(new SupportsVirtualRoutes());
        componentRegistry.Register<IFpleditAttributes>(new FpleditAttributes(typeof(Timetable), "fpl-th", "fpl-dnt", "fpl-tc", "fpl-trc", "fpl-sc", "fpl-sw", "fpl-hw", "fpl-mw", "fpl-tw"));

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
            overrideItem = graphItem.CreateCheckItem(T._("Verwende nur &Plandarstellung"), isChecked: Style.OverrideEntityStyle,
                changeHandler: OverrideItem_CheckedChanged);
#if !DEBUG
        }
#endif
    }

    private void PrintItem_Click(object? sender, EventArgs e)
    {
        try
        {
            new PrintRenderer(pluginInterface).InitPrint();
        }
        catch (Exception ex)
        {
            pluginInterface.Logger.Error(ex.Message);
        }
    }

    private void ExportItem_Click(object? sender, EventArgs e)
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

    private void ShowItem_Click(object? sender, EventArgs e) => dpf?.Show(pluginInterface);

    private void ShowForm(Dialog<DialogResult> form)
    {
        pluginInterface.StageUndoStep();
        if (form.ShowModal((Window)pluginInterface.RootForm) == DialogResult.Ok)
            pluginInterface.SetUnsaved();
        if (!form.IsDisposed)
            form.Dispose();
    }

    private void OverrideItem_CheckedChanged(object? sender, EventArgs e)
    {
        pluginInterface.Settings.Set(OVERRIDE_SETTINGS_KEY, overrideItem.Checked);
        Style.OverrideEntityStyle = overrideItem.Checked;
    }

    private void PluginInterface_FileStateChanged(object? sender, FileStateChangedEventArgs e)
    {
        showItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
        configItem.Enabled = e.FileState.Opened;
        printItem.Enabled = exportItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
        trainColorItem.Enabled = stationStyleItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
    }

    public void Dispose() => dpf?.Dispose();
}