using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();

            string info = string.Format(Properties.Resources.Info, 
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
            label1.Text = info;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://fahrplan.manuelhu.de/");
        }
    }
}
