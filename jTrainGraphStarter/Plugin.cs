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

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Init(IInfo info)
        {
            this.info = info;

            ToolStripMenuItem item = new ToolStripMenuItem("jTG");
            info.Menu.Items.AddRange(new[] { item });
            var showItem = item.DropDownItems.Add("Anzeigen");
            //showItem.Enabled = false;
            showItem.Click += (s, e) => Start();
        }

        public void Start()
        {
            info.Save(false);

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
