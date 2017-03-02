using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
                MessageBox.Show("This installer works only on Windows!");
                return;
            }

            if (MessageBox.Show("Would you like FPLedit as file handler for .fpl?", "FPLedit Installer", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            RegistryKey ckey = Registry
                .ClassesRoot
                .CreateSubKey("fpledit_fpl_file")
                .CreateSubKey("shell")
                .CreateSubKey("open")
                .CreateSubKey("command");

            ckey.SetValue("", "\"F:\\VS-Projects\\Buchfahrplan\\Buchfahrplan\\bin\\Debug\\FPLedit.exe\" \"%1\"");
            ckey.Close();

            RegistryKey fkey = Registry
                .ClassesRoot
                .CreateSubKey(".fpl");
            fkey.SetValue("", "fpledit_fpl_file");
            fkey.Close();

            MessageBox.Show("Installed succesfully!");
        }
    }
}
