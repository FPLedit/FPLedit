using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.Shared.UI.Network;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class VirtualRouteForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;

#pragma warning disable CS0649
        private readonly GridView gridView;
#pragma warning restore CS0649

        public VirtualRouteForm(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            tt = pluginInterface.Timetable;
            
            if (tt.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "virtual routes");

            Eto.Serialization.Xaml.XamlReader.Load(this);

            gridView.AddColumn<VirtualRoute>(t => t.GetRouteName(), T._("Streckenverlauf"));
            gridView.DataStore = VirtualRoute.GetVRoutes(tt).ToArray();

            this.AddCloseHandler();
        }
        
        private void CreateVRoute()
        {
            using var tpf = TrainPathForm.NewTrain(pluginInterface);
            tpf.Title = T._("Verlauf der neuen virtuellen Strecke auswählen");
            if (tpf.ShowModal(this) != DialogResult.Ok)
                return;
            
            VirtualRoute.CreateVRoute(tt, tpf.Path.First(), tpf.Path.Last(), tpf.Waypoints.ToArray());
            gridView.DataStore = VirtualRoute.GetVRoutes(tt).ToArray();
        }

        private void DeleteVRoute(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                VirtualRoute.DeleteVRoute((VirtualRoute) gridView.SelectedItem);
                gridView.DataStore = VirtualRoute.GetVRoutes(tt).ToArray();
            }
            else if (message)
                MessageBox.Show(T._("Zuerst muss eine virtuelle Strecke ausgewählt werden!"), T._("Virtuelle Strecke löschen"));
        }

        private void DeleteButton_Click(object sender, EventArgs e) => DeleteVRoute();
        private void NewButton_Click(object sender, EventArgs e) => CreateVRoute();

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;
            this.NClose();
        }
        
        private static class L
        {
            public static readonly string Delete = T._("Virtuelle Strecke Löschen");
            public static readonly string New = T._("Neue virtuelle Strecke");
            public static readonly string Close = T._("Schließen");
            public static readonly string Title = T._("Virtuelle Strecken bearbeiten");
        }
    }
}
