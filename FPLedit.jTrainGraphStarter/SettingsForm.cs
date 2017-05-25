﻿using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.jTrainGraphStarter
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            javaPathTextBox.Text = SettingsManager.Get("jTGStarter.javapath", "");
            jtgPathTextBox.Text = SettingsManager.Get("jTGStarter.jtgpath", "jTrainGraph_203.jar");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            bool jtgexists = File.Exists(jtgPathTextBox.Text) || ExecutableExists(jtgPathTextBox.Text);
            bool javaexists = ExecutableExists(javaPathTextBox.Text);

            if (!javaexists || !jtgexists)
            {
                var text = "";
                if (!jtgexists)
                    text += "Die angegebene Datei für jTrainGraph wurde nicht gefunden. ";
                if (!javaexists)
                    text += "Java wurde unter dem angegebenen Pfad nicht gefunden. ";
                text += "Wollen Sie trotzdem fortfahren?";
                if (MessageBox.Show(text, "jTrainGraphStarter: Fehler", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            SettingsManager.Set("jTGStarter.javapath", javaPathTextBox.Text);
            SettingsManager.Set("jTGStarter.jtgpath", jtgPathTextBox.Text);
            Close();
        }

        private bool ExecutableExists(string path)
        {
            bool exists = true;
            try
            {
                var p = Process.Start(path);
                p.Kill();
            }
            catch { exists = false; }

            return exists;
        }

        private void docLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/bildfahrplaene/");

        private void downloadLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start("http://kinzigtalbahn.bplaced.net/homepage/programme.html");
    }
}
