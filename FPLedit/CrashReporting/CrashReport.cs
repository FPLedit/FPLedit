using FPLedit.Extensibility;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FPLedit.CrashReporting
{
    public sealed class CrashReport
    {
        public ExceptionInfo Exception { get; set; }
        public ExtensionInfo[] Extensions { get; set; }
        public string Version { get; set; }
        public string OS { get; set; }
        public string[] Assemblies { get; set; }
        public DateTime Time { get; set; }

        internal CrashReport(ExtensionManager mg, Exception x)
        {
            SafeAction(() => Time = DateTime.Now);
            SafeAction(() => Exception = new ExceptionInfo(x));
            SafeAction(() => Extensions = mg.Plugins.Where(p => p.Enabled).Select(p => new ExtensionInfo(p)).ToArray());
            SafeAction(() => Version = FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).ProductVersion);
            SafeAction(() => OS = Environment.OSVersion.ToString());
            SafeAction(() => Assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray());
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

        private void SafeAction(Action action)
        {
            try { action(); } catch { }
        }
    }

    public sealed class ExtensionInfo
    {
        internal ExtensionInfo(PluginInfo plugin)
        {
            Name = plugin.Name;
            Author = plugin.Author;
            Version = plugin.Version;
            Url = plugin.Url;
            FullName = plugin.FullName;
        }

        public string Name { get; }
        public string Author { get; }
        public string Version { get; }
        public string Url { get; }
        public string FullName { get; }
    }

    public sealed class ExceptionInfo
    {
        public ExceptionInfo(Exception x)
        {
            TypeName = x.GetType().FullName;
            Message = x.Message;
            StackTrace = x.StackTrace;
            Source = x.Source;
            InnerException = x.InnerException == null ? null : new ExceptionInfo(x.InnerException);
        }

        public string Message { get; }
        public string StackTrace { get; }
        public string Source { get; }
        public string TypeName { get; }
        public ExceptionInfo InnerException { get; }
    }
}
