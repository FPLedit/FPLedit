using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FPLedit.Shared.UI;

namespace FPLedit.jTrainGraphStarter
{
    internal sealed class RunningForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;

        private bool forceKill, exitClose;

#pragma warning disable CS0649,CA2213
        private readonly ProgressBar progressBar = default!;
#pragma warning restore CS0649,CA2213
        
        public bool JtgSuccess { get; private set; }

        public RunningForm(IPluginInterface pluginInterface, string fnArg, string jtgPath, string javapath)
        {
            this.pluginInterface = pluginInterface;
            
            Eto.Serialization.Xaml.XamlReader.Load(this);

            progressBar.Indeterminate = true;
            
            WindowStyle = WindowStyle.None;
            Closing += (s, e) => e.Cancel = !exitClose;

            var task = ExecuteJTrainGraphTask(fnArg, jtgPath, javapath);
            task.ContinueWith(t => Application.Instance.Invoke(() =>
            {
                JtgSuccess = t.Result;
                exitClose = true;
                Close();
            }), TaskScheduler.Default);
            task.Start();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(T._("jTrainGraph wirklich beenden? Alle in jTrainGraph geänderten Daten werden verworfen! (Das Beenden kann bis zu eine Sekunde dauern)"), MessageBoxButtons.YesNo, MessageBoxType.Warning) == DialogResult.Yes)
            {
                forceKill = true;
                Title = T._("jTrainGraph beenden...");
            }
        }

        private Task<bool> ExecuteJTrainGraphTask(string fnArg, string jtgPath, string javapath)
        {
            return new Task<bool>(() =>
            {
                var jtgFolder = Path.GetDirectoryName(jtgPath);

                using (var p = new Process())
                {
                    p.StartInfo.WorkingDirectory = jtgFolder;
                    p.StartInfo.FileName = javapath;
                    p.StartInfo.Arguments = "-jar \"" + jtgPath + "\" \"" + fnArg + "\"";

                    try
                    {
                        if (!p.Start())
                            throw new Exception("Process could not be started!");

                        pluginInterface.Logger.Info(T._("Wartet darauf, dass jTrainGraph beendet wird..."));

                        while (!p.HasExited)
                        {
                            if (!forceKill)
                                p.WaitForExit(1000);
                            else
                            {
                                p.Kill();
                                return false;
                            }
                        }

                        pluginInterface.Logger.Info(T._("jTrainGraph beendet! Lade Datei neu..."));

                        if (p.ExitCode != 0)
                            throw new Exception("Process exited with error code " + p.ExitCode);
                    }
                    catch (Exception e)
                    {
                        pluginInterface.Logger.Error(T._("Fehler beim Starten von jTrainGraph: Möglicherweise ist das jTrainGraphStarter Plugin falsch konfiguriert! Zur Konfiguration siehe jTrainGraph > Einstellungen"));
                        pluginInterface.Logger.LogException(e);
                        return false;
                    }

                    return true;
                }
            });
        }
    }
}
