using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared.UI;

namespace FPLedit.Templating
{
    internal class TemplateDebugger : FForm
    {
        private readonly TextArea generatedCode;

        public void SetContext(JavascriptTemplate template)
        {
            Visible = false; // Hide until we have an error.
            
            Title = "Generated code: " + template.Identifier;
            
            var line = 1;
            generatedCode.Text = string.Join(Environment.NewLine,template.CompiledCode
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(l => $"/* {line++,4} */  {l}"));
            generatedCode.Selection = new Range<int>(0, 0);
        }

        public void Navigate(int line, int column) // We have an error.
        {
            var lines = generatedCode.Text.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            var start = string.Join(Environment.NewLine, lines.Take(line - 1)).Length + Environment.NewLine.Length;
            var end = string.Join(Environment.NewLine, lines.Take(line)).Length;
            generatedCode.Selection = new Range<int>(start, end);

            OpenDebugger();
        }
        
        public void OpenDebugger() // We have an error, so show this form.
        {
            Show();
            Focus();
        }

        private TemplateDebugger()
        {
            generatedCode = new TextArea
            {
                ReadOnly = true,
                Wrap = false,
                Font = Fonts.Monospace(10),
            };
            Content = generatedCode;
            Size = new Size(800, 800);
        }

        private static TemplateDebugger _instance;
        private static bool _opened;

        public static TemplateDebugger GetInstance()
        {
            if (!_opened)
            {
                _instance = new TemplateDebugger();
                _instance.Closed += (s, e) => _opened = false;
                _opened = true;
            }

            return _instance;
        }
    }
}