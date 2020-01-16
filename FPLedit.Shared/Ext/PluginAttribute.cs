using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        public string Name { get; }

        public string MinVer { get; }

        public string MaxVer { get; }

        public string Author { get; set; }

        public string Web { get; set; }

        public string Version { get; set; }

        public PluginAttribute(string name, string minVer, string maxVer)
        {
            Name = name;
            MinVer = minVer;
            MaxVer = maxVer;
        }
    }
}
