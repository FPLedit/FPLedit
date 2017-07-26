using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        private string _name;
        public string Name => _name;

        public string Author { get; set; }

        public string Web { get; set; }

        public string Version { get; set; }

        public PluginAttribute(string name)
        {
            _name = name;
        }
    }
}
