﻿using System.Collections.Generic;
using Raven.Client;

namespace FubuPersistence.RavenDb
{
    public interface IDocumentStoreBuilder
    {
        IDocumentStore Build();
    }

    public class DocumentStoreBuilder : IDocumentStoreBuilder
    {
        private readonly IEnumerable<IDocumentStoreConfigurationAction> _configurations;
        private readonly RavenDbSettings _settings;

        public DocumentStoreBuilder(RavenDbSettings settings, IEnumerable<IDocumentStoreConfigurationAction> configurations)
        {
            _settings = settings;
            _configurations = configurations;
        }

        public IDocumentStore Build()
        {
            var documentStore = _settings.Create();

            _configurations.Each(x => x.Configure(documentStore));

            documentStore.Initialize();

            return documentStore;
        }
    }
}