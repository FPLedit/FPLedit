using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit
{
    public static class EtoExtensions
    {
        public static Stream GetResource(this Dialog<DialogResult> dialog, string dotFilePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static Stream GetResource(this Form dialog, string dotFilePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static Form GetRootForm(this Dialog<DialogResult> dialog)
        {
            return Program.App.MainForm;
        }
    }
}
