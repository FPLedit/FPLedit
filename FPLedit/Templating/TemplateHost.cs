using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit.Templating
{
    /// <summary>
    /// Exception-Handling for compiled templates.
    /// </summary>
    internal class TemplateHost : ITemplate
    {
        private readonly Template tmpl;
        private readonly ILog logger;

        public string TemplateType => tmpl.TemplateType;

        public string TemplateName => tmpl.TemplateName;

        public string Identifier { get; }

        public string TemplateSource => tmpl.TemplateSource;

        public bool Enabled { get; private set; }

        public TemplateHost(string content, string identifier, ILog log, bool enabled)
        {
            tmpl = new Template(content);
            logger = log;
            Identifier = identifier;
            Enabled = enabled;

            if (tmpl.TemplateType == null)
                logger.Warning("Keine valide Template-Deklaration gefunden! Das Template steht deshalb nicht zur Verfügung!");
        }

        public string GenerateResult(Timetable tt)
        {
            if (!Enabled)
                return null;

            try
            {
                return tmpl.GenerateResult(tt);
            }
            catch (TargetInvocationException ex)
            {
                logger.Error("Laufzeitfehler im Template " + Identifier + ": " + ex.InnerException.Message);
                logger.Error("[StackTrace]: " + ex.InnerException.StackTrace);
            }
            catch (Exception ex)
            {
                logger.Error("Fehler im Template " + Identifier + ": " + ex.Message);
            }
            return null;
        }
    }
}
