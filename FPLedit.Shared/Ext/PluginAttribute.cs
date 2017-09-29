using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        private string _name, _minVer;
        public string Name => _name;
        public string MinVer => _minVer;

        public string Author { get; set; }

        public string Web { get; set; }

        public string Version { get; set; }

        public PluginAttribute(string name, string minVer)
        {
            _name = name;
            _minVer = minVer;
        }
    }
}
