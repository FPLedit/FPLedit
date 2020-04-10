namespace FPLedit.Shared.Tests
{
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