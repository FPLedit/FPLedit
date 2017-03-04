using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Installer
{
    internal class Installer
    {
        public void Install()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                MessageBox.Show("Dieses Installationsprogramm funktioniert nur auf Windows!", "FPLedit Installationsprogramm");
                return;
            }

            if (MessageBox.Show("Wollen Sie .fpl-Dateien standradmäßig per Doppelklick mit FPLedit öffnen?" + Environment.NewLine + Environment.NewLine +
                "Ein späteres Verschieben der FPLedit-Programmdateien führt zum Nichtfuktionieren dieser Funktion; dann muss sie neu eingerichtet werden!",
                "FPLedit Installationsprogramm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            var exe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FPLedit.exe");

            if (!File.Exists(exe))
            {
                MessageBox.Show("FPLedit-Programmdatei nicht gefunden!" + Environment.NewLine + "Pfad: " + exe);
                return;
            }

            RegistryKey ckey = Registry
                .ClassesRoot
                .CreateSubKey("fpledit_fpl_file")
                .CreateSubKey("shell")
                .CreateSubKey("open")
                .CreateSubKey("command");

            ckey.SetValue("", "\"" + exe + "\" \"%1\"");
            ckey.Close();

            RegistryKey fkey = Registry
                .ClassesRoot
                .CreateSubKey(".fpl");
            fkey.SetValue("", "fpledit_fpl_file");
            fkey.Close();

            MessageBox.Show("Installation erfolgreich!", "FPLedit Installationsprogramm");
        }

        public void Uninstall()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                MessageBox.Show("Dieses Installationsprogramm funktioniert nur auf Windows!", "FPLedit Installationsprogramm");
                return;
            }

            if (MessageBox.Show("Wollen Sie die Verknüpfung von .fpl-Dateien mit FPLedit aufheben?",
                "FPLedit Installationsprogramm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            Registry.ClassesRoot.DeleteSubKeyTree("fpledit_fpl_file");
            Registry.ClassesRoot.DeleteSubKeyTree(".fpl");

            MessageBox.Show("Deinstallation erfolgreich!", "FPLedit Installationsprogramm");
        }

        public bool IsInstalled()
        {
            var key = Registry.ClassesRoot.OpenSubKey("fpledit_fpl_file");
            return key != null;
        }
    }
}
