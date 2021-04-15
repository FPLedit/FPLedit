using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using FPLedit.Shared.UI;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class ExportRenderer
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;

        public ExportRenderer(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.tt = pluginInterface.Timetable;
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void InitPrint()
        {
            var form = new FDialog<bool>();
            var routesDropDown = new RoutesDropDown();
            var routeStack = new StackLayout(routesDropDown) { Orientation = Orientation.Horizontal, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            var routeGroup = new GroupBox { Content = routeStack, Text = T._("Strecke auswählen") };

            var widthLabel = new Label { Text = T._("Breite des Bildes (px)") };
            var widthTextBox = new TextBox { Text = "1000" };
            var exportButton = new Button { Text = T._("Exportieren") };
            var printerStack = new StackLayout(widthLabel, widthTextBox, exportButton) { Orientation = Orientation.Horizontal, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            var printerGroup = new GroupBox { Content = printerStack, Text = T._("Exporteinstellungen") };
            
            var stack = new StackLayout(routeGroup, printerGroup) { Orientation = Orientation.Vertical, Padding = new Eto.Drawing.Padding(10), Spacing = 5 };
            
            routesDropDown.Initialize(pluginInterface);
            routesDropDown.EnableVirtualRoutes = true;
            
            form.Content = stack;
            form.DefaultButton = exportButton;
            form.Title = T._("Bildfahrplan drucken");

            exportButton.Click += (s, e) =>
            {
                form.Result = true;
                form.Close();
            };

            if (form.ShowModal())
            {
                var width = 0;
                int.TryParse(widthTextBox.Text, out width);
                if (width == 0)
                {
                    pluginInterface.Logger.Error("Ungültige Breite angegeben!");
                    return;
                }
                
                var exportFileDialog = new SaveFileDialog { Title = T._("Bildfahrplan exportieren") };
                exportFileDialog.Filters.Add(new FileFilter("PNG-Datei", ".png"));
                if (exportFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
                {
                    var filename = exportFileDialog.FileName;
                    var export = new BitmapExport(routesDropDown.SelectedRoute, width, ImageFormat.Png);

                    pluginInterface.Logger.Info(T._("Exportiere Bildfahrplan in Datei {0}", filename));
                    var tsk = export.GetAsyncSafeExport(tt.Clone(), filename, pluginInterface);
                    tsk.ContinueWith((t, o) =>
                    {
                        if (t.Result == false)
                            pluginInterface.Logger.Error(T._("Exportieren des Bildfahrplans fehlgeschlagen!"));
                        else
                            pluginInterface.Logger.Info(T._("Exportieren des Bildfahrplans erfolgreich abgeschlossen!"));
                    }, null, TaskScheduler.Default);
                    tsk.Start(); // Non-blocking operation.
                }
            }
        }
    }
}