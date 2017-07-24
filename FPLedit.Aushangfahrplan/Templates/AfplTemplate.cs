﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion: 15.0.0.0
//  
//     Änderungen an dieser Datei können fehlerhaftes Verhalten verursachen und gehen verloren, wenn
//     der Code neu generiert wird.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace FPLedit.Aushangfahrplan.Templates
{
    using System.Linq;
    using FPLedit.Shared;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    public partial class AfplTemplate : AfplTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("\r\n<!doctype html>\r\n<html>\r\n\t<head>\r\n\t\t<meta charset=\"utf-8\">\r\n<style>\r\nbody {\r\n\tc" +
                    "olor:black;\r\n\tfont-size:16px;\r\n\tfont-family: ");
            
            #line 13 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(font));
            
            #line default
            #line hidden
            this.Write(@", sans-serif;
}
table {
	margin-left: auto;
	margin-right: auto;
	margin-bottom: 50px;
	border: 0;
	border-collapse:collapse;
	border-bottom:3px solid #000;
	page-break-after: always;
}
p {
	font-size:20px;
	text-align:center;
	margin: 0;
	font-weight: normal;
}
.title {
	font-size: 60px;
	letter-spacing: 1.5px;
}
.station {
	font-size: 16px;
	font-stretch: condensed;
	font-weight: bold;
}
.heading {
	margin-bottom: 5px;
	font-weight: bold;
}
td {
	padding-top:8px;
	padding-right:3px;
	padding-left:3px;
	vertical-align:middle;
}
.train-row {
	height:21px;
	border-top:1px solid #000;
}
.train-row ~ .train-row {
	border-top:1px solid #000;
}
.time {
	text-align:center;
	width: 200px;
	border-right: 2px solid #000;
}
.name {
	text-align:center;
	width: 76px;
}
.header {
	border-bottom: 2px solid #000;
}
.time:not(.header), .name:not(.header) {
	font-family: serif;
}
.dir {
	border-top: 3px solid #000;
	border-right: 3px solid #000;
	border-bottom: 1px solid #000;
	text-align: center;
	font-weight:bold;
}
.dir ~ .dir {
	border-right: 0;
}
.time + .name:not(:last-child) {
	border-right:3px solid #000;
}
</style>
<style id=""add-css"">
	");
            
            #line 86 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(additionalCss));
            
            #line default
            #line hidden
            this.Write("\r\n</style>\r\n</head>\r\n<body>\r\n\t");
            
            #line 90 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 foreach (var sta in helper.GetStations()) { 
            
            #line default
            #line hidden
            this.Write("\t<p class=\"station\">Bahnhof ");
            
            #line 91 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(sta.SName));
            
            #line default
            #line hidden
            this.Write("</p>\r\n\t<p class=\"title\">");
            
            #line 92 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(abfahrtSVG));
            
            #line default
            #line hidden
            this.Write("</p>\r\n\t<p class=\"heading\">der Züge in der Richtung nach</p>\r\n\t");
            
            #line 94 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"

		var fs = tt.Stations.First();
		var ls = tt.Stations.Last();
	
            
            #line default
            #line hidden
            this.Write("\t<table>\r\n\t\t<tr>\r\n\t\t\t<td colspan=\"2\" class=\"dir\">");
            
            #line 100 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(fs != sta ? fs.SName : ""));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t\t<td colspan=\"2\" class=\"dir\">");
            
            #line 101 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ls != sta ? ls.SName : ""));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t</tr>\r\n\t\t");
            
            #line 103 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"

			var trainsA = helper.GetTrains(TrainDirection.ta, sta);
			var trainsB = helper.GetTrains(TrainDirection.ti, sta);
			int count = Math.Max(trainsA.Length, trainsB.Length);
			//count = Math.Max(count, 20);
		
            
            #line default
            #line hidden
            this.Write("\t\t<tr>\r\n\t\t\t");
            
            #line 110 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 if (fs != sta) { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time header\">Zeit</td>\r\n\t\t\t\t<td class=\"name header\">Zug-Nr</td>\r\n\t" +
                    "\t\t");
            
            #line 113 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time header\"></td>\r\n\t\t\t\t<td class=\"name header\"></td>\r\n\t\t\t");
            
            #line 116 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t\t");
            
            #line 117 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 if (ls != sta) { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time header\">Zeit</td>\r\n\t\t\t\t<td class=\"name header\">Zug-Nr</td>\r\n\t" +
                    "\t\t");
            
            #line 120 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time header\"></td>\r\n\t\t\t\t<td class=\"name header\"></td>\r\n\t\t\t");
            
            #line 123 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t</tr>\r\n\t\t");
            
            #line 125 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 for (int i = 0; i < count; i++) { 
            
            #line default
            #line hidden
            this.Write("\t\t<tr class=\"train-row\">\r\n\t\t\t");
            
            #line 127 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 if (fs != sta) { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time\">");
            
            #line 128 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(TimeString(trainsA, sta, i)));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t\t\t<td class=\"name\">");
            
            #line 129 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(NameString(trainsA, i)));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t\t");
            
            #line 130 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time\"></td>\r\n\t\t\t\t<td class=\"name\"></td>\r\n\t\t\t");
            
            #line 133 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t\t");
            
            #line 134 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 if (ls != sta) { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time\">");
            
            #line 135 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(TimeString(trainsB, sta, i)));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t\t\t<td class=\"name\">");
            
            #line 136 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(NameString(trainsB, i)));
            
            #line default
            #line hidden
            this.Write("</td>\r\n\t\t\t");
            
            #line 137 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write("\t\t\t\t<td class=\"time\"></td>\r\n\t\t\t\t<td class=\"name\"></td>\r\n\t\t\t");
            
            #line 140 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t\t</tr>\r\n\t\t");
            
            #line 142 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t</table>\r\n\t");
            
            #line 144 "F:\VS-Projects\Buchfahrplan\Buchfahrplan\FPLedit.Aushangfahrplan\Templates\AfplTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("</body>\r\n</html>\r\n");
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
    public class AfplTemplateBase
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