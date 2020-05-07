using FPLedit.Tests.Common.TestClasses;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class RvcTests
    {
        [Test]
        public void EscapeNetworkTest()
        {
            var tt = new Timetable(TimetableType.Network);
            var e = new TestEntity("test", tt);

            e.SetAttribute("rvctest", "0:foo:bar;;baz::;;;;;1:test;2:;");

            var rvc = new RouteValueCollection<string>(e, tt, "rvctest", "default;:test", s => s, s => s, false);

            rvc.TestForErrors(); // Should do nothing

            Assert.AreEqual("foo:bar;baz::;;", rvc.GetValue(0));
            Assert.AreEqual("test", rvc.GetValue(1));
            Assert.AreEqual("default;:test", rvc.GetValue(2));

            Assert.AreEqual("default;:test", rvc.GetValue(3)); // test default value

            rvc.SetValue(0, "");
            rvc.SetValue(1, "test;:test");
            rvc.Write();

            Assert.AreEqual("0:;1:test;;:test;2:default;;:test", e.GetAttribute<string>("rvctest"));
        }
    }
}