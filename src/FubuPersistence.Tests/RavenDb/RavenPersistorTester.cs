﻿using System;
using System.Linq;
using System.Threading;
using FubuPersistence.InMemory;
using FubuPersistence.RavenDb;
using FubuTestingSupport;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using StructureMap;

namespace FubuPersistence.Tests.RavenDb
{
    [TestFixture]
    public class RavenPersistorTester
    {
        private Container container = new Container(new RavenDbRegistry());
        private RavenPersistor persistor;
        private IDocumentStore documents;
        private ISessionBoundary boundary;
        private IContainer nested;

        [SetUp]
        public void SetUp()
        {
            container = new Container(new RavenDbRegistry());
            container.Inject(new RavenDbSettings
            {
                RunInMemory = true
            });

            nested = container.GetNestedContainer();

            documents = nested.GetInstance<IDocumentStore>();
            documents.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;

            persistor = nested.GetInstance<RavenPersistor>();
            boundary = nested.GetInstance<ISessionBoundary>();
        }

        [TearDown]
        public void TearDown()
        {
            nested.Dispose();
        }

        [Test]
        public void load_all()
        {
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new ThirdEntity());

            boundary.SaveChanges();

            persistor.LoadAll<User>().Count().ShouldEqual(3);
            persistor.LoadAll<OtherEntity>().Count().ShouldEqual(2);
            persistor.LoadAll<ThirdEntity>().Count().ShouldEqual(1);
        }

        [Test]
        public void persist()
        {
            var entity = new OtherEntity();

            persistor.Persist(entity);

            boundary.SaveChanges();

            persistor.LoadAll<OtherEntity>().Single().ShouldBeTheSameAs(entity);
        }

        [Test]
        public void delete_all()
        {
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new User());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new OtherEntity());
            persistor.Persist(new ThirdEntity());

            boundary.SaveChanges();

            persistor.DeleteAll<ThirdEntity>();

            boundary.SaveChanges();

            persistor.LoadAll<User>().Count().ShouldEqual(3);
            persistor.LoadAll<OtherEntity>().Count().ShouldEqual(2);
            persistor.LoadAll<ThirdEntity>().Count().ShouldEqual(0);
        }

        [Test]
        public void remove()
        {
            persistor.Persist(new User{Id = Guid.NewGuid()});
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jeremy"
            };
            persistor.Persist(user1);
            persistor.Persist(new User { Id = Guid.NewGuid() });

            boundary.SaveChanges();

            container.GetInstance<RavenTransaction>().Execute<RavenPersistor>(x => {
                var u = x.Find<User>(user1.Id);

                x.Remove(u);
            });

            container.GetInstance<RavenTransaction>()
                .Execute<RavenPersistor>(p => {
                    var users = p.LoadAll<User>().ToList();
                    users.Count().ShouldEqual(2);
                    users.ShouldNotContain(user1);
                });
        }

        [Test]
        public void find_by()
        {
            persistor.Persist(new User());
            persistor.Persist(new User
            {
                FirstName = "Jeremy"
            });
            persistor.Persist(new User());

            boundary.SaveChanges();

            persistor.FindSingle<User>(x => x.FirstName == "Jeremy").FirstName.ShouldEqual("Jeremy");
        }

        [Test]
        public void find_by_gets_the_latest_changes()
        {
            var user1 = new User
            {
                FirstName = "Jeremy"
            };
            persistor.Persist(user1);

            user1.LastName = "Miller";

            boundary.SaveChanges();
        }
    }
}