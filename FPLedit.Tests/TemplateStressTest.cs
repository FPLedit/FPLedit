using System;
using FPLedit.Shared;
using FPLedit.Templating;
using NUnit.Framework;

namespace FPLedit.Tests;

public class TemplateStressTest
{
    [Test]
    [Category("Stress")]
    [Ignore("Stress test - only used to determine properties of the used template system")]
    public void TemplateMemoryTest()
    {
        var tt = new Timetable(TimetableType.Linear);
        var s = string.Empty.PadLeft(1000, 'A');
        var tmpl = new JavascriptTemplate(s, "builtin:testcase", new DummyPluginInterface());
        tmpl.GenerateResult(tt); // Allocate memory used for one computation

        var before = GC.GetTotalMemory(false);
        Assert.IsTrue(before / Math.Pow(2, 20) < 80); // lower than 80 MB

        for (var i = 0; i < 10000; i++)
        {
            tmpl.GenerateResult(tt);
            GC.Collect();
        }

        var after = GC.GetTotalMemory(false);
        Assert.IsTrue(after <= before * 1.2f); // Not more than 120% from the start
    }
}