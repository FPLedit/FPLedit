using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.jTrainGraphStarter
{
#if DEBUG
    public class Plugin : IPlugin
    {
        IInfo info;
        ToolStripItem startItem;

        public string Name
        {
            get
            {
                return "Starter für jTrainGraph";
            }
        }

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            var item = new ToolStripMenuItem("jTG");
            info.Menu.Items.AddRange(new[] { item });
            startItem = item.DropDownItems.Add("jTG Starten");
            startItem.Enabled = false;
            startItem.Click += (s, e) => Start();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            startItem.Enabled = e.FileState.Opened;
        }

        public void Start()
        {
            info.Save(false);
            info.Logger.Info("NUR FÜR TESTZWECKE!");

            //TODO: Appsettings
            string javapath = @"C:\ProgramData\Oracle\Java\javapath\java.exe";
            string jtgPath = @"F:\Software\GrafischerFahrplan\jTrainGraph_201\jTrainGraph_201.jar";
            string jtgFolder = Path.GetDirectoryName(jtgPath);

            Process p = new Process();
            p.StartInfo.FileName = javapath;
            p.StartInfo.WorkingDirectory = jtgFolder;
            p.StartInfo.Arguments = "-jar " + jtgPath + " \""+info.FileState.FileName+"\"";
            p.Start();
            p.WaitForExit();

            info.Reload();
        }
    }
#endif
}
