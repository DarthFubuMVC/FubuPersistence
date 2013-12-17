﻿using System;
using FubuPersistence.RavenDb;
using FubuTestingSupport;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Extensions;

namespace FubuPersistence.Tests.RavenDb
{
    [TestFixture]
    public class RavenDbSettingsTester
    {
        [Test]
        public void builds_in_memory()
        {
            var settings = new RavenDbSettings {RunInMemory = true};
            using (var store = settings.Create())
            {
                store.ShouldBeOfType<EmbeddableDocumentStore>().RunInMemory.ShouldBeTrue();
            }
        }

        [Test]
        public void build_empty_does_not_throw_but_connects_to_the_parallel_data_folder()
        {
            using( var store = new RavenDbSettings().Create())
            {
                store.ShouldBeOfType<EmbeddableDocumentStore>()
                     .DataDirectory.ShouldEndWith("data");
            }

        }

        [Test]
        public void in_memory_is_wait_for_it_in_memory()
        {
            var settings = RavenDbSettings.InMemory();

            var store = settings.Create();
            store.ShouldBeOfType<EmbeddableDocumentStore>().RunInMemory.ShouldBeTrue();

            store.Dispose();
        }

        [Test]
        public void build_in_memory()
        {
            var store = createStore<EmbeddableDocumentStore>(x => x.RunInMemory = true);
            store.RunInMemory.ShouldBeTrue();
            store.UseEmbeddedHttpServer.ShouldBeFalse();

            store.Dispose();
        }

        [Test]
        public void build_using_embedded_http_server_in_memory()
        {
            var store = createStore<EmbeddableDocumentStore>(x =>
            {
                x.RunInMemory = true;
                x.UseEmbeddedHttpServer = true;
            });
            store.RunInMemory.ShouldBeTrue();
            store.UseEmbeddedHttpServer.ShouldBeTrue();

            store.Dispose();
        }

        [Test]
        public void build_using_embedded_http_server_with_data_directory()
        {
            var store = createStore<EmbeddableDocumentStore>(x =>
            {
                x.DataDirectory = "data".ToFullPath();
                x.UseEmbeddedHttpServer = true;
            });
            store.DataDirectory.ShouldEqual("data".ToFullPath());
            store.UseEmbeddedHttpServer.ShouldBeTrue();

            store.Dispose();
        }

        [Test]
        public void build_with_data_directory()
        {
            var store = createStore<EmbeddableDocumentStore>(x => x.DataDirectory = "data".ToFullPath());
            store.DataDirectory.ShouldEqual("data".ToFullPath());
            store.UseEmbeddedHttpServer.ShouldBeFalse();
            store.Dispose();
        }

        [Test]
        public void build_with_url()
        {
            var store = createStore<DocumentStore>(x => x.Url = "http://somewhere:8080");
            store.Url.ShouldEqual("http://somewhere:8080");

            store.Dispose();
        }

        [Test]
        public void is_empty()
        {
            new RavenDbSettings().IsEmpty().ShouldBeTrue();

            new RavenDbSettings
            {
                RunInMemory = true
            }.IsEmpty().ShouldBeFalse();

            new RavenDbSettings
            {
                DataDirectory = "data"
            }.IsEmpty().ShouldBeFalse();

            new RavenDbSettings
            {
                Url = "http://server.com"
            }.IsEmpty().ShouldBeFalse();
        }

        private T createStore<T>(Action<RavenDbSettings> setup) where T : IDocumentStore
        {
            var settings = new RavenDbSettings();
            if (setup != null) setup(settings);
            using (var documentStore = settings.Create())
            {
                return documentStore.ShouldBeOfType<T>();
            }
        }
    }
}