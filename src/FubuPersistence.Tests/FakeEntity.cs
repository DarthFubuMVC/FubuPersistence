namespace FubuPersistence.Tests
{
    public class FakeEntity : Entity { }

    public class User : Entity
    {
        public string FirstName { get; set; }
    }

    public class OtherEntity : Entity { }

    public class ThirdEntity : Entity { }
}