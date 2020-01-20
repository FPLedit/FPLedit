using System;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class DaysTest
    {
        [Test]
        public void InitTest()
        {
            Assert.Throws<ArgumentException>(() => Days.Parse(""));
            Assert.Throws<ArgumentException>(() => Days.Parse("00000"));
            Assert.Throws<ArgumentException>(() => Days.Parse("00000000"));
        }
    }
}