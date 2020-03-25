using FPLedit.Extensibility;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace FPLedit.CrashReporting
{
    public sealed class CrashReport
    {
        public ExceptionInfo Exception { get; set; }
        public ExtensionInfo[] Extensions { get; set; }
        public string Version { get; set; }
        public string VersionFlag { get; set; }
        public string OperatingSystem { get; set; }
        public string RuntimeVersion { get; set; }
        public string[] Assemblies { get; set; }
        public DateTime Time { get; set; }

        internal CrashReport(ExtensionManager mg, Exception x)
        {
            SafeAction(() => Time = DateTime.Now);
            SafeAction(() => Exception = new ExceptionInfo(x));
            SafeAction(() => Extensions = mg.Plugins.Where(p => p.Enabled).Select(p => new ExtensionInfo(p)).ToArray());
            SafeAction(() => Assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray());
            
            // Version information
            SafeAction(() => Version = VersionInformation.BaseVersionString);
            SafeAction(() => VersionFlag = VersionInformation.VersionFlag ?? "");
            SafeAction(() => RuntimeVersion = VersionInformation.RuntimeVersion);
            SafeAction(() => OperatingSystem = VersionInformation.OsVersion);
        }
        
        public CrashReport() {} // needed for Serialization

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
        
        public ExtensionInfo() {} // needed for Serialization

        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string FullName { get; set; }
    }

    public sealed class ExceptionInfo
    {
        public ExceptionInfo() {} // needed for Serialization
        
        public ExceptionInfo(Exception x)
        {
            TypeName = x.GetType().FullName;
            Message = x.Message;
            StackTrace = x.StackTrace;
            Source = x.Source;
            InnerException = x.InnerException == null ? null : new ExceptionInfo(x.InnerException);
        }

        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string TypeName { get; set; }
        public ExceptionInfo InnerException { get; set; }
    }
}
