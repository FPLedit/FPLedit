﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion: 15.0.0.0
//  
//     Änderungen an dieser Datei können fehlerhaftes Verhalten verursachen und gehen verloren, wenn
//     der Code neu generiert wird.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace FPLedit.Kursbuch.Templates
{
    using System.Linq;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    public partial class KfplTemplate : KfplTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("\r\n<!doctype html>\r\n<html>\r\n\t<head>\r\n\t\t<meta charset=\"utf-8\">\r\n\t\t<title>Tabellenfa" +
                    "hrplan generiert von FPLedit</title>\r\n\t\t<style>\r\n\t\tbody {\r\n\t\t\tfont-family:");
            
            #line 12 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(font));
            
            #line default
            #line hidden
            this.Write(@",sans-serif;
		}
		table {
			border-collapse:
			collapse;
			border:none;
			margin-bottom: 20px;
			margin-left: auto;
			margin-right: auto;
		}
		.heading {
			text-align:center;
			margin-top:0cm;
			margin-right:0cm;
			margin-bottom:8.0pt;
			margin-left:0cm;
			line-height:107%;
			font-size:11.0pt;
		}
		.heading .line {
			font-family:");
            
            #line 32 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(heFont));
            
            #line default
            #line hidden
            this.Write(",sans-serif;\r\n\t\t\tfont-weight:900;\r\n\t\t\tletter-spacing:-.5pt;\r\n\t\t}\r\n\t\t.heading .bac" +
                    "k {\r\n\t\t\tletter-spacing:.5pt;\r\n\t\t}\r\n\t\t.kmcap {\r\n\t\t\tborder:solid black 1.0pt;\r\n\t\t\t" +
                    "padding:0;\r\n\t\t}\r\n\t\t.kmcap p {\r\n\t\t\tfont-size:9pt;\r\n\t\t\ttext-align:center;\r\n\t\t}\r\n\t\t" +
                    ".caption.top {\r\n\t\t\tborder-top:solid black 1.0pt;\r\n\t\t}\r\n\t\t.caption.bottom {\r\n\t\t\tb" +
                    "order-bottom:solid black 1.0pt;\r\n\t\t}\r\n\t\t.caption {\r\n\t\t\tborder-right:solid black " +
                    "1.5pt;\r\n\t\t\tpadding:0;\r\n\t\t}\r\n\t\t.caption p {\r\n\t\t\tmargin: 0;\r\n\t\t\tline-height:107%;\r" +
                    "\n\t\t\tfont-size:8.0pt;\r\n\t\t\ttext-align:right;\r\n\t\t\tline-height:normal;\r\n\t\t}\r\n\t\t.tn {" +
                    "\r\n\t\t\tborder-top:solid black 1.5pt;\r\n\t\t\tborder-right:solid black 1.0pt;\r\n\t\t\tpaddi" +
                    "ng: 0 3px 0 3px;\r\n\t\t\ttext-align:center;\r\n\t\t}\r\n\t\t.kl {\r\n\t\t\tborder-bottom:solid bl" +
                    "ack 1pt;\r\n\t\t\tborder-right:solid black 1.0pt;\r\n\t\t\tpadding: 0 3px 0 3px;\r\n\t\t\ttext-" +
                    "align:center;\r\n\t\t}\r\n\t\t.ti {\r\n\t\t\tborder-right:solid black 1.0pt;\r\n\t\t\tpadding: 0 3" +
                    "px 0 3px;\r\n\t\t\ttext-align:center;\r\n\t\t}\r\n\t\t.ti.last {\r\n\t\t\tborder-bottom:solid blac" +
                    "k 1.5pt;\r\n\t\t}\r\n\t\t.sta {\r\n\t\t\tborder-right:solid black 1.5pt;\r\n\t\t\tpadding: 0 3px 0" +
                    " 3px;\r\n\t\t}\r\n\t\t.tn span, .kl span, .ti span {\r\n\t\t\tmargin: 0;\r\n\t\t\tfont-size:8.0pt;" +
                    "\r\n\t\t\tline-height:normal;\r\n\t\t}\r\n\t\t.sta span, .km span {\r\n\t\t\tmargin: 0;\r\n\t\t\tline-h" +
                    "eight:107%;\r\n\t\t\tfont-size:9.0pt;\r\n\t\t\tline-height:normal;\r\n\t\t}\r\n\t\t.sta.last, .km." +
                    "last {\r\n\t\t\tborder-bottom:solid black 1pt;\r\n\t\t}\r\n\t\t.last-t {\r\n\t\t\tborder-right:sol" +
                    "id black 1.5pt;\r\n\t\t}\r\n\t\t.km {\r\n\t\t\tborder-left:solid black 1.0pt;\r\n\t\t\tborder-righ" +
                    "t:solid black 1.0pt;\r\n\t\t\tpadding:0;\r\n\t\t}\r\n\t\t</style>\r\n\t\t<style id=\"add-css\">\r\n\t\t" +
                    "\t");
            
            #line 112 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(additionalCss));
            
            #line default
            #line hidden
            this.Write("\r\n\t\t</style>\r\n\t</head>\r\n\r\n\t<body>\r\n\t<p class=\"heading\">\r\n\t\t<span class=\"line\">");
            
            #line 118 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(kbs));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 118 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(tt.GetLineName(TOP_DIRECTION)));
            
            #line default
            #line hidden
            this.Write(" </span>\r\n\t\t<span class=\"back\">und zurück</span>\r\n\t</p>\r\n\t");
            
            #line 121 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
	var dirs = new[] { TOP_DIRECTION, BOTTOM_DIRECTION };
		foreach (var direction in dirs) {
		var trains = helper.GetTrains(direction);
		var stations = helper.GetStations(direction);
	
            
            #line default
            #line hidden
            this.Write("\t<table cellspacing=\"0\" cellpadding=\"0\">\r\n\t\t<tr>\r\n\t\t\t<td class=\"kmcap\" rowspan=\"2" +
                    "\"><p>km</span></td>\r\n\t\t\t<td class=\"caption top\"><p><span>Zug Nr.</span></p></td>" +
                    "\r\n\t\t\t");
            
            #line 130 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 foreach (var t in trains) {
				var lastt = trains.Last() == t ? " last-t" : "";
			
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"tn");
            
            #line 133 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(lastt));
            
            #line default
            #line hidden
            this.Write("\"><span>");
            
            #line 133 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Sign(t)));
            
            #line default
            #line hidden
            
            #line 133 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(t.TName.Replace(" ", "&nbsp;")));
            
            #line default
            #line hidden
            this.Write("</span></td>\r\n\t\t\t");
            
            #line 134 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t</tr>\r\n\t\t<tr>\r\n\t\t\t<td class=\"caption bottom\"><p><span>Klasse</span></p></td>\r\n\t" +
                    "\t\t");
            
            #line 138 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 foreach (var t in trains) {
				var lastt = trains.Last() == t ? " last-t" : "";
			
            
            #line default
            #line hidden
            this.Write("\t\t\t<td class=\"kl");
            
            #line 141 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(lastt));
            
            #line default
            #line hidden
            this.Write("\"><span>2. oG</span></td>\r\n\t\t\t");
            
            #line 142 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t</tr>\r\n\t\t");
            
            #line 144 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"

		foreach (var sta in stations) { 
            
            #line default
            #line hidden
            this.Write("\t\t<tr>\r\n\t\t\t");
            
            #line 147 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 var last = stations.Last() == sta ? " last" : ""; 
            
            #line default
            #line hidden
            this.Write("\t\t\t<td class=\"km");
            
            #line 148 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(last));
            
            #line default
            #line hidden
            this.Write("\"><span>");
            
            #line 148 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(sta.Kilometre.ToString("0.0")));
            
            #line default
            #line hidden
            this.Write("</span></td>\r\n\t\t\t<td class=\"sta");
            
            #line 149 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(last));
            
            #line default
            #line hidden
            this.Write("\"><span>");
            
            #line 149 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(sta.SName.Replace(" ", "&nbsp;")));
            
            #line default
            #line hidden
            this.Write("</span></td>\r\n\r\n\t\t\t");
            
            #line 151 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 foreach (var t in trains) {
				var times = "";
				var time = t.GetArrDep(sta).Departure;
				if (time == default(TimeSpan)) time =  t.GetArrDep(sta).Arrival;
				if (time == default(TimeSpan)) times = "...";
				else times = time.ToString(@"hh\:mm");
				last += trains.Last() == t ? " last-t" : "";
			
            
            #line default
            #line hidden
            this.Write("\t\t\t<td class=\"ti");
            
            #line 159 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(last));
            
            #line default
            #line hidden
            this.Write("\"><span>");
            
            #line 159 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(times));
            
            #line default
            #line hidden
            this.Write("</span></td>\r\n\t\t\t");
            
            #line 160 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t</tr>\r\n\t\t");
            
            #line 162 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t</table>\r\n\t");
            
            #line 164 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Kursbuch\Templates\KfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n\t</body>\r\n\r\n</html>");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    public class KfplTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
