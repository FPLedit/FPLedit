using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using FPLedit.Shared.Filetypes;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class FileTests
    {
        [Test]
        public void NamespaceTest()
        {
            var text = Load("test_xmlns.fpl");
            var el = XElement.Parse(text);
            Assert.Throws<NotSupportedException>(() => new XMLEntity(el));

            Assert.Throws<LogErrorException>(() => new XMLImport().Import(PrepareTemp(text), new DummyPluginInterface()));
            
            // Empty file
            Assert.Throws<LogErrorException>(() => new XMLImport().Import(PrepareTemp(""), new DummyPluginInterface()));
        }

        private string Load(string dotPath) => ResourceHelper.GetStringResource("Shared.Tests.TestFiles." + dotPath);

        private string PrepareTemp(string text)
        {
            var rand = new Random();
            var path = Path.Combine(Path.GetTempPath(), "fpledit-test-" + rand.Next(100) + ".fpl");
            File.WriteAllText(path, text);
            return path;
        }
    }
}