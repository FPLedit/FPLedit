using FPLedit.Extensibility;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FPLedit.CrashReporting
{
    public class CrashReport
    {
        public ExceptionInfo Exception { get; set; }

        public ExtensionInfo[] Extensions { get; set; }

        public string Version { get; set; }

        public string OS { get; set; }

        public string[] Assemblies { get; set; }

        public DateTime Time { get; set; }

        public CrashReport()
        {

        }

        internal CrashReport(ExtensionManager mg, Exception x)
        {
            Time = DateTime.Now;
            Exception = new ExceptionInfo(x);
            Extensions = mg.Plugins.Where(p => p.Enabled).Select(p => new ExtensionInfo(p)).ToArray();
            Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            OS = Environment.OSVersion.ToString();
            Assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray();
        }

        public string Serialize()
        {
            var s = new XmlSerializer(typeof(CrashReport));
            using (var stream = new MemoryStream())
            {
                s.Serialize(stream, this);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    public class ExtensionInfo
    {
        public ExtensionInfo()
        {
        }

        internal ExtensionInfo(PluginInfo info)
        {
            Name = info.Name;
            Author = info.Author;
            Version = info.Version;
            Url = info.Url;
            FullName = info.FullName;
        }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Url { get; set; }

        public string FullName { get; set; }
    }

    public class ExceptionInfo
    {
        public ExceptionInfo()
        {

        }

        public ExceptionInfo(Exception x)
        {
            Message = x.Message;
            StackTrace = x.StackTrace;
            Source = x.Source;
            InnerException = x.InnerException == null ? null : new ExceptionInfo(x.InnerException);
        }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string Source { get; set; }

        public ExceptionInfo InnerException { get; set; }
    }
}
