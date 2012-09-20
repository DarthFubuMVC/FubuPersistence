using FubuPersistence.InMemory;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace FubuPersistence.Tests.InMemory
{
    [TestFixture]
    public class InMemoryPersistorTester
    {
        [Test]
        public void load_all()
        {
            var persistor = new InMemoryPersistor();

            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new ThirdEntity());

            persistor.LoadAll<User>().Count().ShouldEqual(3);
            persistor.LoadAll<OtherEntity>().Count().ShouldEqual(2);
            persistor.LoadAll<ThirdEntity>().Count().ShouldEqual(1);
        }

        [Test]
        public void persist()
        {
            var entity = new OtherEntity();
            var persistor = new InMemoryPersistor();

            persistor.Persist(entity);

            persistor.LoadAll<OtherEntity>().Single().ShouldBeTheSameAs(entity);
        }

        [Test]
        public void delete_all()
        {
            var persistor = new InMemoryPersistor();

            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new ThirdEntity());

            persistor.DeleteAll<ThirdEntity>();

            persistor.LoadAll<User>().Count().ShouldEqual(3);
            persistor.LoadAll<OtherEntity>().Count().ShouldEqual(2);
            persistor.LoadAll<ThirdEntity>().Count().ShouldEqual(0);
        }

        [Test]
        public void remove()
        {
            var persistor = new InMemoryPersistor();

            persistor.Persist(new User());
            var user1 = new User();
            persistor.Persist(user1);
            persistor.Persist(new User());

            persistor.Remove(user1);

            persistor.LoadAll<User>().Count().ShouldEqual(2);
            persistor.LoadAll<User>().ShouldNotContain(user1);
        }

        [Test]
        public void find_by()
        {
            var persistor = new InMemoryPersistor();

            persistor.Persist(new User());
            persistor.Persist(new User
            {
                FirstName = "Jeremy"
            });
            persistor.Persist(new User());

            persistor.FindBy<User>(x => x.FirstName == "Jeremy").FirstName.ShouldEqual("Jeremy");
        }
    }
}