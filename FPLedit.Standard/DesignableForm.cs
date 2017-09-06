using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class DesignableForm : Form
    {
        private IInfo info;
        private List<ISaveHandler> saveHandlers;

        private DesignableForm()
        {
            InitializeComponent();
            saveHandlers = new List<ISaveHandler>();
        }

        public DesignableForm(IInfo info) : this()
        {
            this.info = info;
        }

        private void DesignableForm_Load(object sender, EventArgs e)
        {
            var designables = info.GetRegistered<IDesignableUiProxy>();

            tabControl1.SuspendLayout();
            tabControl1.TabPages.Clear();

            foreach (var d in designables)
            {
                var c = d.GetControl(info);
                var tp = new TabPage(d.DisplayName);
                c.BackColor = tp.BackColor;
                tp.AutoScroll = true;
                tp.Controls.Add(c);
                tabControl1.TabPages.Add(tp);

                if (c is ISaveHandler sh)
                    saveHandlers.Add(sh);
            }

            tabControl1.ResumeLayout();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            saveHandlers.ForEach(sh => sh.Save());
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
