﻿using System;
using FubuPersistence.RavenDb;
using FubuPersistence.RavenDb.Multiple;
using FubuTestingSupport;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using StructureMap;

namespace FubuPersistence.Tests.RavenDb.Integration
{
    [TestFixture]
    public class plugging_in_a_custom_interface_for_an_additional_database
    {
        private Container theContainer;

        [SetUp]
        public void SetUp()
        {
            theContainer = new Container(x =>
            {
                x.ConnectToRavenDb<FourthDbSettings>(store =>
                {
                    store.Conventions.DefaultQueryingConsistency = ConsistencyOptions.MonotonicRead;
                }).Using<IFourthDatabase, FourthDatabase>();


                x.IncludeRegistry<RavenDbRegistry>();
                x.For<RavenDbSettings>().Use(RavenDbSettings.InMemory);
                x.For<FourthDbSettings>().Use(new FourthDbSettings { RunInMemory = true });
            });
        }

        [TearDown]
        public void TearDown()
        {
            theContainer.Dispose();
        }

        [Test]
        public void session_boundary_respects_transaction_boundaries()
        {
            var foo1 = new Foo { Id = Guid.NewGuid(), Name = "Jeremy" };

            var transaction = theContainer.GetInstance<ITransaction>();
            transaction.Execute<IFourthDatabase>(x => x.Store(foo1));

            transaction.Execute<IFourthDatabase>(session =>
            {
                session.Load<Foo>(foo1.Id).Name.ShouldEqual("Jeremy");
            });

            transaction.Execute<IFourthDatabase>(session =>
            {
                session.Load<Foo>(foo1.Id).Name.ShouldEqual("Jeremy");
            });


        }
    }

    public class FourthDbSettings : RavenDbSettings
    {

    }

    public interface IFourthDatabase : IDocumentSession<FourthDbSettings> { }

    public class FourthDatabase : DocumentSession<FourthDbSettings>, IFourthDatabase
    {
        public FourthDatabase(ISessionBoundary boundary) : base(boundary)
        {
        }
    }
}