using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using Jint.Runtime;

namespace FPLedit.Templating
{
    /// <summary>
    /// Exception-Handling for compiled templates.
    /// </summary>
    internal class TemplateHost : ITemplate
    {
        private readonly ITemplate tmpl;
        private readonly ILog logger;

        public string TemplateType => tmpl?.TemplateType;

        public string TemplateName => tmpl?.TemplateName;

        public string Identifier { get; }

        public string TemplateSource => tmpl?.TemplateSource;

        public bool Enabled { get; private set; }

        public TemplateHost(string content, string identifier, IInfo info, bool enabled)
        {
            logger = info.Logger;
            Identifier = identifier;
            Enabled = enabled;
            
            try
            {
                tmpl = new JavascriptTemplate(content, identifier, info);
                
                if (tmpl?.TemplateType == null)
                    logger.Warning(
                        "Keine valide Template-Deklaration gefunden! Das Template steht deshalb nicht zur Verfügung!");
            }
            catch (Exception ex)
            {
                logger.Error("Init-Fehler im Template " + Identifier + ": " + ex.Message);
            }
        }

        public string GenerateResult(Timetable tt)
        {
            if (!Enabled || tmpl == null)
                return null;

            try
            {
                return tmpl.GenerateResult(tt);
            }
            catch (JavaScriptException ex)
            {
                logger.Error($"Fehler im Template {Identifier}: {ex.Message} in line {ex.LineNumber}, column {ex.Column}");
                TemplateDebugger.GetInstance().Navigate(ex.LineNumber, ex.Column);
            }
            catch (Exception ex)
            {
                logger.Error("Fehler im Template " + Identifier + ": " + ex.Message);
                TemplateDebugger.GetInstance().OpenDebugger();
            }

            return null;
        }
    }
}