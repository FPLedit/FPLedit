using System;
using System.Xml.Linq;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Tests.TestClasses;
using FPLedit.Tests.Common;
using NUnit.Framework;

namespace FPLedit.Shared.Tests;

public sealed class FileTests : BaseFileTests
{
    [Test]
    public void NamespaceTest()
    {
        var text = Load("test_xmlns.fpl");
        var el = XElement.Parse(text);
        Assert.Throws<NotSupportedException>(() => new XMLEntity(el));

        using var s = PrepareTemp(text);
        Assert.Throws<NotSupportedException>(() => new XMLImport().Import(s, new DummyPluginInterface()));
    }

    [Test]
    public void EmptyFileTest()
    {
        using var s = PrepareTemp("");
        Assert.Throws<System.Xml.XmlException>(() => new XMLImport().Import(s, new DummyPluginInterface()));
    }
}