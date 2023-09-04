using FPLedit.Shared.Helpers;
using NUnit.Framework;

namespace FPLedit.Shared.Tests;

public class TimeNormalizerTest
{
    [Test]
    public void NormalizeTest()
    {
        var normalizer = new TimeNormalizer();
            
        Assert.AreEqual("00:00", normalizer.Normalize("0:0"));
        Assert.AreEqual("00:00", normalizer.Normalize("0"));
        Assert.AreEqual("00:00", normalizer.Normalize("00:00"));
        Assert.AreEqual("00:00", normalizer.Normalize("0:00"));
        Assert.AreEqual("00:00", normalizer.Normalize("00:0"));
        Assert.AreEqual("10:02", normalizer.Normalize("10:2"));
        Assert.AreEqual("10:02", normalizer.Normalize("10:02"));
        Assert.AreEqual("02:10", normalizer.Normalize("2:10"));
        Assert.AreEqual("02:01", normalizer.Normalize("2:1"));
        Assert.AreEqual("00:01", normalizer.Normalize(":1"));
        Assert.AreEqual("00:01", normalizer.Normalize(":01"));
        Assert.AreEqual("24:00", normalizer.Normalize("24:00"));
        Assert.AreEqual("24:00", normalizer.Normalize("24:0"));
        Assert.AreEqual("24:01", normalizer.Normalize("24:01"));
        Assert.AreEqual("00:00", normalizer.Normalize(":"));
        Assert.AreEqual("01:02", normalizer.Normalize("1:2"));
        Assert.AreEqual("12:00", normalizer.Normalize("12:"));
        Assert.AreEqual("05:00", normalizer.Normalize("5:"));
        Assert.AreEqual("05:30", normalizer.Normalize("530"));
        Assert.AreEqual("00:36", normalizer.Normalize("36"));
        Assert.AreEqual("00:06", normalizer.Normalize("6"));
        Assert.AreEqual("15:36", normalizer.Normalize("1536"));
            
        Assert.AreEqual("00:00", normalizer.Normalize("00:00:00")); // Remove empty seconds part.
        Assert.AreEqual("01:04", normalizer.Normalize("01:04:00")); // Remove empty seconds part.
        Assert.AreEqual("01:00:02", normalizer.Normalize("1::2"));

            
        Assert.AreEqual(null, normalizer.Normalize(" "));
        Assert.AreEqual(null, normalizer.Normalize(""));
        Assert.AreEqual(null, normalizer.Normalize("1 2"));
        Assert.AreEqual(null, normalizer.Normalize("12345"));
        Assert.AreEqual(null, normalizer.Normalize("::"));
        Assert.AreEqual(null, normalizer.Normalize("a123"));
        Assert.AreEqual(null, normalizer.Normalize("a"));
        Assert.AreEqual(null, normalizer.Normalize("12a"));
    }
}