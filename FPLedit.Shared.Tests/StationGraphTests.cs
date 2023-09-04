using System;
using System.Xml.Linq;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Tests.TestClasses;
using FPLedit.Tests.Common;
using NUnit.Framework;

namespace FPLedit.Shared.Tests;

public sealed class StationGraphTests : BaseFileTests
{
    [Test]
    public void CycleTests()
    {
        var text = Load("test_cycles.fpl");
        using var s = PrepareTemp(text);

        var tt = new XMLImport().Import(s, new DummyPluginInterface());
        Assert.IsNotNull(tt);
        Assert.IsTrue(tt!.Initialized);

        Assert.IsTrue(tt.HasRouteCycles);
    }
}