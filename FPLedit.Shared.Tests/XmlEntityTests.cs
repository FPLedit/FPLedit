using System;
using System.Xml.Linq;
using NUnit.Framework;

namespace FPLedit.Shared.Tests;

public class XmlEntityTests
{
    [Test]
    public void CreationTest()
    {
        var x = new XMLEntity("test");
        Assert.AreEqual("test", x.XName);
        Assert.AreEqual(0, x.Attributes.Count);
        Assert.AreEqual(0, x.Children.Count);
        Assert.AreEqual(null, x.Value);

        // externes setzen
        x.SetAttribute("test-attr1", "test-attr1-val");
        Assert.IsTrue(x.Attributes.Count == 1);
        Assert.IsTrue(x.Attributes.ContainsKey("test-attr1"));
        Assert.AreEqual("test-attr1-val", x.Attributes["test-attr1"]);
        Assert.AreEqual("test-attr1-val", x.GetAttribute<string>("test-attr1"));

        // internes setzen
        Assert.AreEqual(null, x.GetAttribute<string>("test-attr2"));
        Assert.AreEqual(0, x.GetAttribute<int>("test-attr2"));
        x.Attributes["test-attr2"] = "2";
        Assert.AreEqual("2", x.GetAttribute<string>("test-attr2"));
        Assert.AreEqual(2, x.GetAttribute<int>("test-attr2"));

        // externes Löschen
        x.RemoveAttribute("test-attr1");
        Assert.AreEqual(null, x.GetAttribute<string>("test-attr1"));
        Assert.IsFalse(x.Attributes.ContainsKey("test-attr1"));

        // internes Löschen
        x.Attributes.Remove("test-attr2");
        Assert.AreEqual(0, x.GetAttribute<int>("test-attr2"));

        // Value
        x.Value = "testval";
        Assert.AreEqual("testval", x.Value);

        // Children (simple)
        var x2 = new XMLEntity("c");
        x.Children.Add(x2);
        Assert.AreEqual(1, x.Children.Count);
        Assert.AreEqual("c", x.Children[0].XName);
        x.Children.RemoveAt(0);
        Assert.AreEqual(0, x.Children.Count);
    }

    [Test]
    public void XElementTest()
    {
        // xmlns
        var el = XElement.Parse("<fpl:test xmlns:fpl=\"test\" />");
        Assert.Throws<NotSupportedException>(() => new XMLEntity(el));

        // basic
        el = XElement.Parse("<test a=\"1\" b=\"aBcDeF\" />");
        var en = new XMLEntity(el);
        Assert.AreEqual("test", en.XName);
        Assert.AreEqual(2, en.Attributes.Count);
        Assert.AreEqual("1", en.Attributes["a"]);
        Assert.AreEqual("aBcDeF", en.Attributes["b"]);

        // children
        el = XElement.Parse("<test a=\"1\" b=\"aBcDeF\"><a abc=\"1\" /><b>d</b>hallo</test>");
        en = new XMLEntity(el);
        Assert.AreEqual("hallo", en.Value);
        Assert.AreEqual(2, en.Attributes.Count); // attributes again
        Assert.AreEqual("1", en.Attributes["a"]);
        Assert.AreEqual("aBcDeF", en.Attributes["b"]);
        Assert.AreEqual(2, en.Children.Count);
        Assert.AreEqual("a", en.Children[0].XName);
        Assert.AreEqual("1", en.Children[0].Attributes["abc"]);
        Assert.AreEqual(null, en.Children[0].Value);
        Assert.AreEqual("b", en.Children[1].XName);
        Assert.AreEqual("d", en.Children[1].Value);
    }
}