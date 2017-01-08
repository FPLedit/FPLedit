using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            javaPathTextBox.Text = SettingsManager.Get("jTGStarter.javapath", "java");
            jtgPathTextBox.Text = SettingsManager.Get("jTGStarter.jtgpath", "jTrainGraph_202.jar");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            SettingsManager.Set("jTGStarter.javapath", javaPathTextBox.Text);
            SettingsManager.Set("jTGStarter.jtgpath", jtgPathTextBox.Text);
            Close();
        }
    }
}
