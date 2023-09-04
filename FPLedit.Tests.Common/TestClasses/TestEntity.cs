using FPLedit.Shared;

namespace FPLedit.Tests.Common.TestClasses;

public class TestEntity : Entity
{
    public TestEntity(string xn, Timetable tt) : base(xn, tt)
    {
    }

    public TestEntity(XMLEntity en, Timetable tt) : base(en, tt)
    {
    }
}