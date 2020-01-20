using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class EntityTests
    {
        [Test]
        public void CreationTest()
        {
            var tt = new Timetable(TimetableType.Linear);
            
            var x = new TestEntity("test", tt);
            Assert.AreEqual("test", x.XMLEntity.XName);
            Assert.AreEqual(0, x.Attributes.Count);
            Assert.AreEqual(0, x.Children.Count);
            Assert.AreEqual(0, x.XMLEntity.Attributes.Count);
            Assert.AreEqual(0, x.XMLEntity.Children.Count);
            Assert.AreEqual(null, x.XMLEntity.Value);
            Assert.AreEqual(tt, x._parent);
            
            // externes setzen
            x.SetAttribute("test-attr1", "test-attr1-val");
            Assert.IsTrue(x.Attributes.Count == 1);
            Assert.IsTrue(x.Attributes.ContainsKey("test-attr1"));
            Assert.AreEqual("test-attr1-val", x.Attributes["test-attr1"]);
            Assert.AreEqual("test-attr1-val", x.GetAttribute<string>("test-attr1"));
            
            Assert.IsTrue(x.XMLEntity.Attributes.Count == 1);
            Assert.IsTrue(x.XMLEntity.Attributes.ContainsKey("test-attr1"));
            Assert.AreEqual("test-attr1-val", x.XMLEntity.Attributes["test-attr1"]);
            Assert.AreEqual("test-attr1-val", x.XMLEntity.GetAttribute<string>("test-attr1"));
            
            // internes setzen
            Assert.AreEqual(null, x.GetAttribute<string>("test-attr2"));
            Assert.AreEqual(0, x.GetAttribute<int>("test-attr2"));
            Assert.AreEqual(null, x.XMLEntity.GetAttribute<string>("test-attr2"));
            Assert.AreEqual(0, x.XMLEntity.GetAttribute<int>("test-attr2"));
            x.Attributes["test-attr2"] = "2";
            Assert.AreEqual("2", x.GetAttribute<string>("test-attr2"));
            Assert.AreEqual(2, x.GetAttribute<int>("test-attr2"));
            Assert.AreEqual("2", x.XMLEntity.GetAttribute<string>("test-attr2"));
            Assert.AreEqual(2, x.XMLEntity.GetAttribute<int>("test-attr2"));
            
            // externes Löschen
            x.RemoveAttribute("test-attr1");
            Assert.AreEqual(null, x.GetAttribute<string>("test-attr1"));
            Assert.IsFalse(x.Attributes.ContainsKey("test-attr1"));
            Assert.AreEqual(null, x.XMLEntity.GetAttribute<string>("test-attr1"));
            Assert.IsFalse(x.XMLEntity.Attributes.ContainsKey("test-attr1"));
            
            // internes Löschen
            x.Attributes.Remove("test-attr2");
            Assert.AreEqual(0, x.GetAttribute<int>("test-attr2"));
            Assert.AreEqual(0, x.XMLEntity.GetAttribute<int>("test-attr2"));
            
            // Collections tauschen
            x.SetAttribute("test", "1");
            x.Attributes = new Dictionary<string, string>();
            Assert.AreEqual(null, x.GetAttribute<string>("test"));
            Assert.IsEmpty(x.XMLEntity.Attributes);

            // Children (simple)
            var x2 = new XMLEntity("c");
            x.Children.Add(x2);
            Assert.AreEqual(1, x.Children.Count);
            Assert.AreEqual("c", x.Children[0].XName);
            Assert.AreEqual(1, x.XMLEntity.Children.Count);
            Assert.AreEqual("c", x.XMLEntity.Children[0].XName);
            x.Children.RemoveAt(0);
            Assert.AreEqual(0, x.Children.Count);
            Assert.AreEqual(0, x.XMLEntity.Children.Count);
            
            // Collections tauschen
            x.Children.Add(x2);
            x.Children = new List<XMLEntity>();
            Assert.IsEmpty(x.Children);
            Assert.IsEmpty(x.XMLEntity.Attributes);
        }

        [Test]
        public void XmlEntityTest()
        {
            var tt = new Timetable(TimetableType.Linear);
            
            // children
            var el = XElement.Parse("<test a=\"1\" b=\"aBcDeF\"><a abc=\"1\" /><b>d</b>hallo</test>");
            var x = new XMLEntity(el);
            var en = new TestEntity(x, tt);
            Assert.AreEqual(2, en.Attributes.Count); // attributes again
            Assert.AreEqual("1", en.Attributes["a"]);
            Assert.AreEqual("aBcDeF", en.Attributes["b"]);
            Assert.AreEqual(2, en.Children.Count);
            Assert.AreEqual("a", en.Children[0].XName);
            Assert.AreEqual("1", en.Children[0].Attributes["abc"]);
            Assert.AreEqual(null, en.Children[0].Value);
            Assert.AreEqual("b", en.Children[1].XName);
            Assert.AreEqual("d", en.Children[1].Value);
            Assert.AreEqual(tt, en._parent);
        }

        public class TestEntity : Entity
        {
            public TestEntity(string xn, Timetable tt) : base(xn, tt)
            {
            }

            public TestEntity(XMLEntity en, Timetable tt) : base(en, tt)
            {
            }
        }
    }
}