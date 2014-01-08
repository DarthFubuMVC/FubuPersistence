﻿using System;
using System.Diagnostics;
using FubuPersistence.RavenDb.Multiple;
using FubuPersistence.Reset;
using Raven.Client;
using StructureMap;
using System.Linq;
using FubuCore;
using System.Collections.Generic;

namespace FubuPersistence.RavenDb
{
    public class RavenPersistenceReset : IPersistenceReset
    {
        private readonly IContainer _container;

        public RavenPersistenceReset(IContainer container)
        {
            _container = container;
        }

        public void ClearPersistedState()
        {
            _container.Model.For<IDocumentStore>().Default.EjectObject();
            _container.Inject(new RavenDbSettings
            {
                RunInMemory = true,
                UseEmbeddedHttpServer = true
            });

            // Force the container to spin it up now just in case other things
            // are trying access the store remotely
            var store = _container.GetInstance<IDocumentStore>();
            Debug.WriteLine("Opening up a new in memory RavenDb IDocumentStore at " + store.Url);


            var otherSettingTypes = FindOtherSettingTypes();

            otherSettingTypes.Each(type => {
                var settings = Activator.CreateInstance(type).As<RavenDbSettings>();
                settings.Url = null;
                settings.ConnectionString = null;
                settings.RunInMemory = true;

                _container.Inject(type, settings);

                var documentStoreType = typeof (IDocumentStore<>).MakeGenericType(type);
                _container.Model.For(documentStoreType).Default.EjectObject();

                // Force the container to spin it up now just in case other things
                // are trying access the store remotely
                _container.GetInstance(documentStoreType);
            });

        }

        public IList<Type> FindOtherSettingTypes()
        {
            var otherSettingTypes = _container.Model.PluginTypes.Where(x => x.PluginType.IsConcreteTypeOf<RavenDbSettings>() && x.PluginType != typeof(RavenDbSettings))
                                              .Select(x => x.PluginType).ToList();
            return otherSettingTypes;
        }

        public void CommitAllChanges()
        {
            // no-op for now
        }

        public static void Try()
        {
            
        }
    }
}