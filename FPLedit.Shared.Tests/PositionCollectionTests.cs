using System;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class PositionCollectionTests
    {
        [Test]
        public void NetworkWriteTest()
        {
            var tt = new Timetable(TimetableType.Network);
            var s = new Station(tt);
            
            var pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            pos.SetPosition(123, 11.3f);
            Assert.AreEqual(11.3f, pos.GetPosition(123));
            pos.Write();
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual("123:11.3", s.Attributes["km"]);
            
            pos.SetPosition(222, 0f);
            pos.Write();
            Assert.AreEqual("123:11.3;222:0.0", s.Attributes["km"]);
            
            // Network has no right/left
            Assert.AreEqual(false, s.Attributes.ContainsKey("kml"));
            Assert.AreEqual(false, s.Attributes.ContainsKey("kmr"));
        }
        
        [Test]
        public void NetworkReadTest()
        {
            var tt = new Timetable(TimetableType.Network);
            var s = new Station(tt) {Attributes = {["km"] = "123:3.9;124:12.0"}};

            var pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual(3.9f, pos.GetPosition(123));
            Assert.AreEqual(12.0f, pos.GetPosition(124));
            
            // with trailing semicolon
            s = new Station(tt) {Attributes = {["km"] = "123:3.9;124:12.0;"}};
            pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual(3.9f, pos.GetPosition(123));
            Assert.AreEqual(12.0f, pos.GetPosition(124));
            
            // Containing ints as position
            s = new Station(tt) {Attributes = {["km"] = "123:3.9;124:12;"}};
            pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual(3.9f, pos.GetPosition(123));
            Assert.AreEqual(12.0f, pos.GetPosition(124));
            
            // Invalid
            s = new Station(tt) {Attributes = {["km"] = "123:3.9;124:12;#"}};
            Assert.Throws<FormatException>(() => new PositionCollection(s, tt));

            // Network has no right/left
            Assert.AreEqual(false, s.Attributes.ContainsKey("kml"));
            Assert.AreEqual(false, s.Attributes.ContainsKey("kmr"));
        }

        /*[Test]
        public void LinearJtg2WriteTest()
        {
            var tt = new Timetable(TimetableType.Linear);
            tt.SetVersion(TimetableVersion.JTG2_x);
            var s = new Station(tt);
            
            var pos = new PositionCollection(s, tt);
            pos.SetPosition(Timetable.LINEAR_ROUTE_ID, 123.4f);
            pos.Write();
            
            Assert.AreEqual("123.4", s.Attributes["km"]);
            
            // Other write does not affect data
            pos.SetPosition(1, 0f);
            pos.Write();
            
            Assert.AreEqual("123.4", s.Attributes["km"]);
            
            // Linear/Jtg2 has no right/left
            Assert.AreEqual(false, s.Attributes.ContainsKey("kml"));
            Assert.AreEqual(false, s.Attributes.ContainsKey("kmr"));
        }
        
        [Test]
        public void LinearJtg2ReadTest()
        {
            var tt = new Timetable(TimetableType.Linear);
            tt.SetVersion(TimetableVersion.JTG2_x);

            // with trailing semicolon
            var s = new Station(tt) {Attributes = {["km"] = "123.4"}};
            var pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual(123.4f, pos.GetPosition(Timetable.LINEAR_ROUTE_ID));
            
            // Containing ints as position
            s = new Station(tt) {Attributes = {["km"] = "123"}};
            pos = new PositionCollection(s, tt);
            pos.TestForErrors(); // Should do nothing
            Assert.AreEqual(123f, pos.GetPosition(Timetable.LINEAR_ROUTE_ID));
            
            // Invalid
            s = new Station(tt) {Attributes = {["km"] = "123;"}};
            Assert.Throws<FormatException>(() => new PositionCollection(s, tt));
            s = new Station(tt) {Attributes = {["km"] = "123a"}};
            Assert.Throws<FormatException>(() => new PositionCollection(s, tt));
        }*/

        [Test]
        public void LinearJtg3WriteTest()
        {
            var versions = new[] {TimetableVersion.JTG3_0, TimetableVersion.JTG3_1};
            foreach (var version in versions)
            {
                var tt = new Timetable(TimetableType.Linear);
                tt.SetVersion(version);
                var s = new Station(tt);
                
                var pos = new PositionCollection(s, tt);
                pos.SetPosition(Timetable.LINEAR_ROUTE_ID, 123.4f);
                pos.Write();
            
                Assert.AreEqual("123.4", s.Attributes["kml"]);
                Assert.AreEqual("123.4", s.Attributes["kmr"]);
            
                // Other write does not affect data
                pos.SetPosition(1, 0f);
                pos.Write();
            
                Assert.AreEqual("123.4", s.Attributes["kml"]);
                Assert.AreEqual("123.4", s.Attributes["kmr"]);
            
                // Linear/Jtg3 has no km
                Assert.AreEqual(false, s.Attributes.ContainsKey("km"));
            }
        }
        
        [Test]
        public void LinearJtg3ReadTest()
        {
            var versions = new[] {TimetableVersion.JTG3_0, TimetableVersion.JTG3_1};
            foreach (var version in versions)
            {
                var tt = new Timetable(TimetableType.Linear);
                tt.SetVersion(version);

                // with trailing semicolon
                var s = new Station(tt) {Attributes = {["kml"] = "1.0", ["kmr"] = "1.0"}};
                var pos = new PositionCollection(s, tt);
                pos.TestForErrors(); // Should do nothing
                Assert.AreEqual(1.0f, pos.GetPosition(Timetable.LINEAR_ROUTE_ID));
            
                // Containing ints as position
                s = new Station(tt) {Attributes = {["kml"] = "123", ["kmr"] = "123"}};
                pos = new PositionCollection(s, tt);
                pos.TestForErrors(); // Should do nothing
                Assert.AreEqual(123f, pos.GetPosition(Timetable.LINEAR_ROUTE_ID));
            
                // Invalid
                s = new Station(tt) {Attributes = {["kml"] = "123;", ["kmr"] = "123;"}};
                Assert.Throws<FormatException>(() => new PositionCollection(s, tt));
                s = new Station(tt) {Attributes = {["kml"] = "123a", ["kmr"] = "123a"}};
                Assert.Throws<FormatException>(() => new PositionCollection(s, tt));
                
                // different
                s = new Station(tt) {Attributes = {["kml"] = "123", ["kmr"] = "124"}};
                Assert.Throws<NotSupportedException>(() => new PositionCollection(s, tt));
            }
        }
    }
}