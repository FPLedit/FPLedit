using System.IO;
using System.Linq;
using FPLedit.Shared.Analyzers;
using FPLedit.Shared.Filetypes;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class AnalyzerTests : BaseFileTests
    {
        [Test]
        public void SimpleLinearCrossingTest()
        {
            using (var s = PrepareTemp(Load("test_crossing0.fpl")))
                GeneralCrossingTest(s);
        }

        [Test]
        public void SimpleNetworkCrossingTest()
        {
            using (var s = PrepareTemp(Load("test_crossing1.fpl")))
                GeneralCrossingTest(s);
        }
        
        [Test]
        public void ComplexNetworkCrossingTest()
        {
            using (var s = PrepareTemp(Load("test_crossing2.fpl")))
                GeneralCrossingTest(s);
        }
        
        [Test]
        public void SimpleLinearTrapezTest()
        {
            using (var s = PrepareTemp(Load("test_trapez0.fpl")))
                GeneralTrapezTest(s);
        }
        
        [Test]
        public void SimpleNetworkTrapezTest()
        {
            using (var s = PrepareTemp(Load("test_trapez1.fpl")))
                GeneralTrapezTest(s);
        }

        [Test]
        public void ComplexNetworkTrapezTest()
        {
            using (var s = PrepareTemp(Load("test_trapez2.fpl")))
                GeneralTrapezTest(s);
        }

        [Test]
        public void SimpleLinearOvertakeTest()
        {
            using (var s = PrepareTemp(Load("test_overtake0.fpl")))
                GeneralOvertakeTest(s);
        }

        [Test]
        public void SimpleNetworkOvertakeTest()
        {
            using (var s = PrepareTemp(Load("test_overtake1.fpl")))
                GeneralOvertakeTest(s);
        }
        
        // No ComplexNetworkOvertakeTest with test_overtake2.fpl as overtaking can only happen on the same route.

        private static void GeneralTrapezTest(Stream s)
        {
            var tt = new XMLImport().Import(s, new DummyPluginInterface());

            var analyzer = new IntersectionAnalyzer(tt);

            var station = tt.Stations.Single(st => st.SName == "B");

            // Normal
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-normal");
                var trapez0 = analyzer.TrapezAtStation(probeTrain, station);
                Assert.AreEqual(0, trapez0.IntersectingTrainsStopping.Length);
                Assert.IsTrue(trapez0.IsStopping);
                Assert.AreEqual(Days.Parse("1111100"), trapez0.StopDays);
            }
            // Normal (reverse)
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "C-normal");
                var trapez1 = analyzer.TrapezAtStation(probeTrain, station);
            
                Assert.AreEqual(1, trapez1.IntersectingTrainsStopping.Length);
                Assert.AreEqual("P-normal", trapez1.IntersectingTrainsStopping[0].TName);
                Assert.IsFalse(trapez1.IsStopping);
                Assert.AreEqual(Days.Parse("1111100"), trapez1.StopDays);
            }
            
            // Single train
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-single");
                var trapez0 = analyzer.TrapezAtStation(probeTrain, station);
                Assert.AreEqual(0, trapez0.IntersectingTrainsStopping.Length);
                Assert.IsTrue(trapez0.IsStopping);
                Assert.AreEqual("1111100", trapez0.StopDays.ToBinString());
            }
            
            // Multiple
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-multiple");
                var trapez0 = analyzer.TrapezAtStation(probeTrain, station);
                Assert.AreEqual(1, trapez0.IntersectingTrainsStopping.Length);
                Assert.AreEqual("C-multiple", trapez0.IntersectingTrainsStopping[0].TName);
                Assert.IsTrue(trapez0.IsStopping);
                Assert.AreEqual(Days.Parse("1111100"), trapez0.StopDays);
                
                var trapez1 = analyzer.TrapezAtStation(trapez0.IntersectingTrainsStopping[0], station);

                Assert.AreEqual(2, trapez1.IntersectingTrainsStopping.Length);
                Assert.AreEqual("P-multiple", trapez1.IntersectingTrainsStopping[0].TName);
                Assert.IsTrue(trapez1.IsStopping);
                Assert.AreEqual(Days.Parse("1111100"), trapez1.StopDays);
            }
        }

        private static void GeneralCrossingTest(Stream s)
        {
            var tt = new XMLImport().Import(s, new DummyPluginInterface());
            
            var analyzer = new IntersectionAnalyzer(tt);

            var station = tt.Stations.Single(st => st.SName == "B");
            
            // Overlap at end
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-end");
                var crossingWith = analyzer.CrossingAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-end", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.CrossingAtStation(crossingWith[0], station).Single());
            }

            // Overlap at begin
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-begin");
                var crossingWith = analyzer.CrossingAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-begin", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.CrossingAtStation(crossingWith[0], station).Single());
            }

            // Overlap in the middle
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-middle");
                var crossingWith = analyzer.CrossingAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-middle", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.CrossingAtStation(crossingWith[0], station).Single());
            }

            // Second train with no stop time
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-no-minutes");
                var crossingWith = analyzer.CrossingAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-no-minutes", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.CrossingAtStation(crossingWith[0], station).Single());
            }

            // Second train with no stop time (two trains)
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-no-minutes-double");
                var crossingWith = analyzer.CrossingAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(2, crossingWith.Length);
                Assert.AreEqual("C-no-minutes-double1", crossingWith[0].TName);
                Assert.AreEqual("C-no-minutes-double2", crossingWith[1].TName);

                foreach (var cw in crossingWith)
                    Assert.AreEqual(probeTrain, analyzer.CrossingAtStation(cw, station).Single());
            }
        }
        
        private static void GeneralOvertakeTest(Stream s)
        {
            var tt = new XMLImport().Import(s, new DummyPluginInterface());
            
            var analyzer = new IntersectionAnalyzer(tt);

            var station = tt.Stations.Single(st => st.SName == "B");
            
            // Overlap at end
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-end");
                var crossingWith = analyzer.OvertakeAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-end", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.OvertakeAtStation(crossingWith[0], station).Single());
            }

            // Overlap at begin
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-begin");
                var crossingWith = analyzer.OvertakeAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-begin", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.OvertakeAtStation(crossingWith[0], station).Single());
            }

            // Overlap in the middle
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-overlap-middle");
                var crossingWith = analyzer.OvertakeAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-overlap-middle", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.OvertakeAtStation(crossingWith[0], station).Single());
            }

            // Second train with no stop time
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-no-minutes");
                var crossingWith = analyzer.OvertakeAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(1, crossingWith.Length);
                Assert.AreEqual("C-no-minutes", crossingWith[0].TName);

                Assert.AreEqual(probeTrain, analyzer.OvertakeAtStation(crossingWith[0], station).Single());
            }

            // Second train with no stop time (two trains)
            {
                var probeTrain = tt.Trains.Single(t => t.TName == "P-no-minutes-double");
                var crossingWith = analyzer.OvertakeAtStation(probeTrain, station).ToArray();
                Assert.AreEqual(2, crossingWith.Length);
                Assert.AreEqual("C-no-minutes-double1", crossingWith[0].TName);
                Assert.AreEqual("C-no-minutes-double2", crossingWith[1].TName);
            }
        }
    }
}