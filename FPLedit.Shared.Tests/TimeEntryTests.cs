using System;
using NUnit.Framework;
// ReSharper disable EqualExpressionComparison

namespace FPLedit.Shared.Tests
{
    public class TimeEntryTests
    {
        [Test]
        public void InitTest()
        {
            var timeEntry = new TimeEntry();
            Assert.AreEqual(timeEntry, TimeEntry.Zero);
            
            Assert.AreEqual(TimeEntry.Zero, new TimeEntry());
            Assert.AreEqual(TimeEntry.Zero, default(TimeEntry));

            Assert.Throws<ArgumentException>(() => new TimeEntry(0, 0, 10, 10));
            
            timeEntry = new TimeEntry(10, 20);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(20, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(620, timeEntry.GetTotalMinutes());

            timeEntry = new TimeEntry(10, 80);
            Assert.AreEqual(11, timeEntry.Hours);
            Assert.AreEqual(20, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(680, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 60);
            Assert.AreEqual(11, timeEntry.Hours);
            Assert.AreEqual(0, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(660, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 59);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(59, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(659, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 61);
            Assert.AreEqual(11, timeEntry.Hours);
            Assert.AreEqual(1, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(661, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(24, 0);
            Assert.AreEqual(24, timeEntry.Hours);
            Assert.AreEqual(0, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(1440, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(24, 61);
            Assert.AreEqual(25, timeEntry.Hours);
            Assert.AreEqual(1, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(1501, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(0, 1440);
            Assert.AreEqual(24, timeEntry.Hours);
            Assert.AreEqual(0, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(1440, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(-10, -20);
            Assert.AreEqual(-10, timeEntry.Hours);
            Assert.AreEqual(-20, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(-620, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(0, -5);
            Assert.AreEqual(0, timeEntry.Hours);
            Assert.AreEqual(-5, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(-5, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, -5);
            Assert.AreEqual(9, timeEntry.Hours);
            Assert.AreEqual(55, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(595, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(-10, -60);
            Assert.AreEqual(-11, timeEntry.Hours);
            Assert.AreEqual(0, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(-660, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(-10, -80);
            Assert.AreEqual(-11, timeEntry.Hours);
            Assert.AreEqual(-20, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(-680, timeEntry.GetTotalMinutes());

            timeEntry = new TimeEntry(-10, 60);
            Assert.AreEqual(-9, timeEntry.Hours);
            Assert.AreEqual(0, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(-540, timeEntry.GetTotalMinutes());
            
            
            // Seconds
            timeEntry = new TimeEntry(10, 20, 13, 0);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(20, timeEntry.Minutes);
            Assert.AreEqual(13, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(620, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 20, 63, 0);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(21, timeEntry.Minutes);
            Assert.AreEqual(3, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(621, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 20, -83, 0);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(18, timeEntry.Minutes);
            Assert.AreEqual(37, timeEntry.Seconds);
            Assert.AreEqual(0, timeEntry.Decimals);
            Assert.AreEqual(618, timeEntry.GetTotalMinutes());
            
            // Decimals
            timeEntry = new TimeEntry(10, 20, 0, 10);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(20, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(10, timeEntry.Decimals);
            Assert.AreEqual(620, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 20, 0, 123);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(21, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(23, timeEntry.Decimals);
            Assert.AreEqual(621, timeEntry.GetTotalMinutes());
            
            timeEntry = new TimeEntry(10, 20, 0, -345);
            Assert.AreEqual(10, timeEntry.Hours);
            Assert.AreEqual(16, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(55, timeEntry.Decimals);
            Assert.AreEqual(616, timeEntry.GetTotalMinutes());
            
            // Overflow
            timeEntry = new TimeEntry(10, -124, 0, -345);
            Assert.AreEqual(7, timeEntry.Hours);
            Assert.AreEqual(52, timeEntry.Minutes);
            Assert.AreEqual(0, timeEntry.Seconds);
            Assert.AreEqual(55, timeEntry.Decimals);
        }

        [Test]
        public void CalcTest()
        {
            // Test unary operators
            var te = new TimeEntry(10, 40);
            var mte = new TimeEntry(-10, -40);
            Assert.AreEqual(te, +te);
            Assert.AreEqual(TimeEntry.Zero, te - (+te));
            Assert.AreEqual(TimeEntry.Zero, te + (-te));
            Assert.AreEqual(TimeEntry.Zero - te, -te);
            Assert.AreEqual(mte, -te);
            Assert.AreEqual(te, -mte);
            
            // Test binary ops
            var dte = new TimeEntry(21, 20);
            Assert.AreEqual(dte, te + te);
            Assert.AreEqual(TimeEntry.Zero, te - te);
            var te2 = new TimeEntry(3, 33);
            Assert.AreEqual(te + te2, te2 + te);
            Assert.AreEqual( te - te2, -(te2 - te));

            Assert.AreEqual(te, dte - te);
            Assert.AreEqual(te, te + te - te);
            Assert.AreEqual(te, te + (te - te));
            Assert.AreEqual(te, (te + te) - te);
            Assert.AreEqual(te, te + TimeEntry.Zero);
            Assert.AreEqual(te, te - TimeEntry.Zero);
            
            // Test if methods do the same
            Assert.AreEqual(te.Add(te2), te + te2);
            Assert.AreEqual(te.Substract(te2), te - te2);
            
            // Test calcualtions with decimals and seconds
            // Test precise calculations
            Assert.AreEqual(new TimeEntry(0, 0, 50, 0), new TimeEntry(0, 0, 37, 0) + new TimeEntry(0, 0, 13, 0));
            Assert.AreEqual(new TimeEntry(0, 0, 0, 50), new TimeEntry(0, 0, 0, 37) + new TimeEntry(0, 0, 0, 13));
            
            // Loss of precision
            Assert.AreEqual(new TimeEntry(0, 0, 0, 74), new TimeEntry(0, 0, 37, 0) + new TimeEntry(0, 0, 0, 13));
        }

        [Test]
        public void CompareTest()
        {
            var te = new TimeEntry(10, 40);
            var te3 = new TimeEntry(10, 30);
            var mte = new TimeEntry(-10, -40);
            var dte = new TimeEntry(21, 20);
            var te2 = new TimeEntry(3, 33);
            
            // Discable CS warning "comparison with the same variable"
            #pragma warning disable CS1718
            Assert.AreEqual(true, dte == te + te);
            Assert.AreEqual(true, te == te);
            Assert.AreEqual(true, te >= te);
            Assert.AreEqual(true, te <= te);
            Assert.AreEqual(true, dte != te);
            Assert.AreEqual(false, te != te);
            Assert.AreEqual(true, te2 < te);
            Assert.AreEqual(true, te2 <= te);
            Assert.AreEqual(true, dte > te);
            Assert.AreEqual(true, dte >= te);
            
            Assert.AreEqual(true, te.Equals(te));
            Assert.AreEqual(true, dte.Equals(te + te));
            Assert.AreEqual(true, !te.Equals(new object()));
            Assert.AreEqual(true, !te.Equals(null));
            Assert.AreEqual(true, !TimeEntry.Zero.Equals(null));
            
            Assert.AreEqual(0, te.CompareTo(te));
            Assert.AreEqual(true, te2.CompareTo(te) < 0);
            Assert.AreEqual(true, te3.CompareTo(te) < 0);
            Assert.AreEqual(true, dte.CompareTo(te) > 0);
            Assert.AreEqual(1, dte.CompareTo(null));
            Assert.Throws<ArgumentException>(() => te.CompareTo(new object()));
            #pragma warning restore CS1718
        }

        [Test]
        public void ToStringTest()
        {
            var timeEntry = new TimeEntry(10, 80, 0, 0);
            Assert.AreEqual("11:20", timeEntry.ToString());
            Assert.AreEqual("11:20", timeEntry.ToShortTimeString());
            
            timeEntry = new TimeEntry(10, 60);
            Assert.AreEqual("11:00", timeEntry.ToString());
            Assert.AreEqual("11:00", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(10, 59);
            Assert.AreEqual("10:59", timeEntry.ToString());
            Assert.AreEqual("10:59", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(10, 61);
            Assert.AreEqual("11:01", timeEntry.ToString());
            Assert.AreEqual("11:01", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(24, 0);
            Assert.AreEqual("24:00", timeEntry.ToString());
            Assert.AreEqual("24:00", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(24, 61);
            Assert.AreEqual("25:01", timeEntry.ToString());
            Assert.AreEqual("25:01", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(0, 1440);
            Assert.AreEqual("24:00", timeEntry.ToString());
            Assert.AreEqual("24:00", timeEntry.ToTimeString(false));
            
            Assert.AreEqual("00:00", TimeEntry.Zero.ToString());
            Assert.AreEqual("00:00", TimeEntry.Zero.ToTimeString(false));
            
            timeEntry = new TimeEntry(10, 59, 0, 10);
            Assert.AreEqual("10:59,10", timeEntry.ToString());
            Assert.AreEqual("10:59", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(10, 59, 10, 0);
            Assert.AreEqual("10:59:10", timeEntry.ToString());
            Assert.AreEqual("10:59", timeEntry.ToTimeString(false));
            
            timeEntry = new TimeEntry(10, 59, 0, 10);
            Assert.AreEqual("10:59", timeEntry.ToTimeString(false));
            Assert.AreEqual("10:59", timeEntry.ToTimeString(false));
        }

        [Test]
        public void ParseTests()
        {
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("0:0"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse(":"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("0"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("00:0"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("0:00"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("00:00"));
            Assert.AreEqual(new TimeEntry(24, 0), TimeEntry.Parse("24:00"));
            Assert.AreEqual(new TimeEntry(24, 0), TimeEntry.Parse("24:0"));
            Assert.AreEqual(new TimeEntry(24, 1), TimeEntry.Parse("24:01"));
            Assert.AreEqual(new TimeEntry(26, 1), TimeEntry.Parse("26:01"));
            Assert.AreEqual(TimeEntry.Zero, TimeEntry.Parse("0:0:0"));
            
            Assert.AreEqual(new TimeEntry(24, 0, 10, 0), TimeEntry.Parse("24:00:10"));
            Assert.AreEqual(new TimeEntry(24, 0, 1, 0), TimeEntry.Parse("24:0:1"));
            Assert.AreEqual(new TimeEntry(24, 1, 33, 0), TimeEntry.Parse("24:01:33"));
            Assert.AreEqual(new TimeEntry(26, 1, 25, 0), TimeEntry.Parse("26:01:25"));

            Assert.AreEqual(new TimeEntry(24, 0, 0, 10), TimeEntry.Parse("24:00,10"));
            Assert.AreEqual(new TimeEntry(24, 0, 0, 10), TimeEntry.Parse("24:0,1"));
            Assert.AreEqual(new TimeEntry(24, 1, 0, 33), TimeEntry.Parse("24:01,33"));
            Assert.AreEqual(new TimeEntry(26, 1, 0, 25), TimeEntry.Parse("26:01,25"));
            
            Assert.Throws<FormatException>(() => TimeEntry.Parse(""));
            Assert.Throws<FormatException>(() => TimeEntry.Parse("."));
            Assert.Throws<FormatException>(() => TimeEntry.Parse("a"));
            Assert.Throws<FormatException>(() => TimeEntry.Parse("abc"));
            Assert.Throws<FormatException>(() => TimeEntry.Parse("10:22:10,5"));
            
            // Dont accept TimeSpan formats (days)
            Assert.Throws<FormatException>(() => TimeEntry.Parse("1.0:0"));
            
            // Try parse
            TimeEntry o;
            Assert.AreEqual(true, TimeEntry.TryParse("0:0", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse("0", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse("00:0", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse("0:00", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse("00:00", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse(":", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:00", out o));
            Assert.AreEqual(new TimeEntry(24, 0), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:0", out o));
            Assert.AreEqual(new TimeEntry(24, 0), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:01", out o));
            Assert.AreEqual(new TimeEntry(24, 1), o);
            Assert.AreEqual(true, TimeEntry.TryParse("26:01", out o));
            Assert.AreEqual(new TimeEntry(26, 1), o);
            Assert.AreEqual(true, TimeEntry.TryParse("0:0:0", out o));
            Assert.AreEqual(TimeEntry.Zero, o);
            
            Assert.AreEqual(true, TimeEntry.TryParse("24:00:10", out o));
            Assert.AreEqual(new TimeEntry(24, 0, 10, 0), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:0:1", out o));
            Assert.AreEqual(new TimeEntry(24, 0, 1, 0), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:01:33", out o));
            Assert.AreEqual(new TimeEntry(24, 1, 33, 0), o);
            Assert.AreEqual(true, TimeEntry.TryParse("26:01:25", out o));
            Assert.AreEqual(new TimeEntry(26, 1, 25, 0), o);
            
            Assert.AreEqual(true, TimeEntry.TryParse("24:00,10", out o));
            Assert.AreEqual(new TimeEntry(24, 0, 0, 10), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:0,1", out o));
            Assert.AreEqual(new TimeEntry(24, 0, 0, 10), o);
            Assert.AreEqual(true, TimeEntry.TryParse("24:01,33", out o));
            Assert.AreEqual(new TimeEntry(24, 1, 0, 33), o);
            Assert.AreEqual(true, TimeEntry.TryParse("26:01,25", out o));
            Assert.AreEqual(new TimeEntry(26, 1, 0, 25), o);
            
            Assert.AreEqual(false, TimeEntry.TryParse("", out o));
            Assert.AreEqual(false, TimeEntry.TryParse(".", out o));
            Assert.AreEqual(false, TimeEntry.TryParse("a", out o));
            Assert.AreEqual(false, TimeEntry.TryParse("abc", out o));
            
            // Dont accept TimeSpan formats (days)
            Assert.AreEqual(false, TimeEntry.TryParse("1.0:0", out o));
        }
    }
}