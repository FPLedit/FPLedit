using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Templating
{
    internal class TemplateHost : ITemplate
    {
        private const string MSG_TITLE = "FPLedit - Templatefehler";

        private Template tmpl;
        private ILog logger;

        public TemplateHost(string content, ILog log)
        {
            tmpl = new Template(content);
            logger = log;

            if (tmpl.TemplateType == null)
                logger.Warning("Keine valide Template-Deklaration gefunden! Das Template steht deshalb nicht zur Verfügung!");
        }

        public string TemplateType => tmpl.TemplateType;

        public string GenerateResult(Timetable tt)
        {
            try
            {
                var result = tmpl.GenerateResult(tt);
                return result;
            }
            catch (TargetInvocationException ex)
            {
                //TODO: Umstellen auf Logger
                MessageBox.Show("Laufzeitfehler im Template: " + ex.InnerException.Message, MSG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MSG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }
    }
}
