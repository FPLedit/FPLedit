using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    internal class TemplateHost
    {
        public string Run(string fn, Timetable tt)
        {
            Template tmpl = new Template(File.ReadAllText(fn));
            try
            {
                var result = tmpl.GenerateResult(tt);
                MessageBox.Show(result);
                return result;
            }
            catch (TargetInvocationException ex)
            {
                MessageBox.Show("Laufzeitfehler im Template: " + ex.InnerException.Message, "FPLedit - Templatefehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FPLedit - Templatefehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }
    }
}
