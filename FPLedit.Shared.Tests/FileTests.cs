using System;
using System.IO;
using System.Text;
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

            using (var s = PrepareTemp(text))
                Assert.Throws<LogErrorException>(() => new XMLImport().Import(s, new DummyPluginInterface()));
        }

        [Test]
        public void EmptyFileTest()
        {
            using (var s = PrepareTemp(""))
                Assert.Throws<LogErrorException>(() => new XMLImport().Import(s, new DummyPluginInterface()));
        }

        [Test]
        public void DuplicateStationIdTest()
        {
            var text = Load("test_duplicate_ids.fpl");
            using (var s = PrepareTemp(text))
            {
                var tt = new XMLImport().Import(s, new DummyPluginInterface());
                Assert.IsNotNull(tt);
                //Assert.IsTrue(tt.UpgradeMessage.Contains("Verknüpfungen"));

                // TODO: Check for changed ids, removed transitions (first: use 
            }
        }

        private string Load(string dotPath) => ResourceHelper.GetStringResource("Shared.Tests.TestFiles." + dotPath);

        private Stream PrepareTemp(string text)
        {
            var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 1024, true))
                sw.Write(text);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}