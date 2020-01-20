using System;
using System.ComponentModel;
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
        }

        [Test]
        public void EmptyFileTest()
        {
            Assert.Throws<LogErrorException>(() => new XMLImport().Import(PrepareTemp(""), new DummyPluginInterface()));
        }

        [Test]
        public void DuplicateStationIdTest()
        {
            var text = Load("test_duplicate_ids.fpl");
            Timetable tt = new XMLImport().Import(PrepareTemp(text), new DummyPluginInterface());
            Assert.IsNotNull(tt);
            Assert.IsTrue(tt.UpgradeMessage.Contains("Verknüpfungen"));
            
            
            // TODO: Check for changed ids, removed transitions
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