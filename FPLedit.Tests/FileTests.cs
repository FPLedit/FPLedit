using System.IO;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.TimetableChecks;
using NUnit.Framework;

namespace FPLedit.Tests
{
    public class FileTests
    {
        [Test]
        public void DuplicateStationIdTest()
        {
            var text = Load("test_duplicate_ids.fpl");
            using (var s = PrepareTemp(text))
            {
                var pi = new DummyPluginInterface();
                pi.Registry.Register<ITimetableInitAction>(new BugFixInitAction());
                
                var tt = new XMLImport().Import(s, pi);
                Assert.IsNotNull(tt);
                
                Assert.IsTrue(pi.HadWarning("Verknüpfungen zu Folgezügen aufgehoben werden") > 0);
                
                Assert.AreEqual(1, tt.Transitions.Count);
                Assert.AreEqual(null, tt.GetTransition(10)); // Transitions got removed.
                Assert.AreEqual("69724", tt.GetTrainById(12).TName); // One id got changed.
                Assert.AreEqual(5, tt.Trains.Count); // We still have 5 trains.
                Assert.AreEqual(5, tt.Trains.Select(t => t.Id).Distinct().Count()); // We have 5 different IDs now.
            }
        }
        
        [Test]
        public void AmbiguousRoutesTest()
        {
            var text = Load("test_ambiguous_routes.fpl");
            using (var s = PrepareTemp(text))
            {
                var pi = new DummyPluginInterface();
                pi.Registry.Register<ITimetableInitAction>(new BugFixInitAction());
                
                var tt = new XMLImport().Import(s, pi);
                Assert.IsNotNull(tt);
                
                Assert.IsTrue(pi.HadWarning("enthält zusammengfefallene Strecken") > 0);
            }
        }
        
        private string Load(string dotPath) => ResourceHelper.GetStringResource("Tests.TestFiles." + dotPath);

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