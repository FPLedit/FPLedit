using System.IO;
using Eto.Forms;
using FPLedit.GTFS.Forms;
using FPLedit.GTFS.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.UI;

namespace FPLedit.GTFS
{
    [Plugin("Modul für GTFS-Export", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
#pragma warning disable CS8618
        private ButtonMenuItem gtfsItem, agencyPropsItem, trainPropsItem, exportItem;
        private IPluginInterface pluginInterface;
#pragma warning restore CS8618

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;

            componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);
            componentRegistry.Register<ISupportsVirtualRoutes>(new SupportsVirtualRoutes());

            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            gtfsItem = ((MenuBar) pluginInterface.Menu).CreateItem(T._("&GTFS"));
            agencyPropsItem = gtfsItem.CreateItem(T._("GTFS-&Agencydaten ändern"), enabled: false, clickHandler: (_, _) => ShowForm(new AgencyPropsForm(pluginInterface)));
            trainPropsItem = gtfsItem.CreateItem(T._("GTFS-&Zugeinstellungen ändern"), enabled: false, clickHandler: (_, _) => ShowForm(new TrainPropsForm(pluginInterface)));
            exportItem = gtfsItem.CreateItem(T._("&Exportiere GTFS-Feed"), enabled: false, clickHandler: (_, _) => DoGtfsExport(pluginInterface.FileState.FileName!));
        }

        private void PluginInterface_FileStateChanged(object? sender, FileStateChangedEventArgs e)
        {
            agencyPropsItem.Enabled = trainPropsItem.Enabled = exportItem.Enabled =
                e.FileState is { Opened: true, TrainsCreated: true, FileName: not null } && pluginInterface.Timetable.Type == TimetableType.Linear;
        }

        private void DoGtfsExport(string filename)
        {
            var attrs = GtfsAttrs.GetAttrs(pluginInterface.Timetable);
            if (attrs == null || !attrs.HasAllRequiredFields)
                ShowForm(new AgencyPropsForm(pluginInterface));

            using var sfd = new SelectFolderDialog();
            sfd.Directory = Path.GetDirectoryName(filename);
            if (sfd.ShowDialog((Window) pluginInterface.RootForm) != DialogResult.Ok) return;
            GtfsExport.Export(pluginInterface.Timetable, pluginInterface.Logger, filename, sfd.Directory!);
        }

        private void ShowForm(Dialog<DialogResult> form)
        {
            pluginInterface.StageUndoStep();
            if (form.ShowModal((Window)pluginInterface.RootForm) == DialogResult.Ok)
                pluginInterface.SetUnsaved();
            if (!form.IsDisposed)
                form.Dispose();
        }

        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer(T._("GTFS"), GtfsAttrs.GetAttrs, GtfsAttrs.CreateAttrs);
    }
}
