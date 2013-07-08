﻿using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using StructureMap;
using StructureMap.Configuration.DSL;
using FubuPersistence.RavenDb;
using FubuTestingSupport;

namespace FubuPersistence.Tests.RavenDb
{
    [TestFixture]
    public class LambdaDocumentStoreConfigurationTester
    {
        [Test]
        public void registers_and_uses_a_lambda_configuration_action()
        {
            var registry = new Registry();
            registry.RavenDbConfiguration(store => {
                store.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;
            });

            var container = new Container(x => {
                x.IncludeRegistry<RavenDbRegistry>();
                x.IncludeRegistry(registry);
                x.For<RavenDbSettings>().Use(RavenDbSettings.InMemory);
            });

            container.GetInstance<IDocumentStore>().Conventions
                     .DefaultQueryingConsistency.ShouldEqual(ConsistencyOptions.QueryYourWrites);
        }

        [Test]
        public void registers_and_uses_a_lambda_configuration_action_2()
        {
            var registry = new Registry();
            registry.RavenDbConfiguration(store =>
            {
                store.Conventions.DefaultQueryingConsistency = ConsistencyOptions.MonotonicRead;
            });

            var container = new Container(x =>
            {
                x.IncludeRegistry<RavenDbRegistry>();
                x.IncludeRegistry(registry);
                x.For<RavenDbSettings>().Use(RavenDbSettings.InMemory);
            });

            container.GetInstance<IDocumentStore>().Conventions
                     .DefaultQueryingConsistency.ShouldEqual(ConsistencyOptions.MonotonicRead);
        }
    }
}