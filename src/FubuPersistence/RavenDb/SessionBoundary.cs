using System;
using System.Collections.Generic;
using System.Diagnostics;
using FubuCore.Util;
using FubuPersistence.RavenDb.Multiple;
using Raven.Client;
using StructureMap;

namespace FubuPersistence.RavenDb
{
    public class SessionBoundary : ISessionBoundary
    {
        private readonly IDocumentStore _store;

        private Lazy<IDocumentSession> _session;
        private readonly Cache<Type, IDocumentSession> _otherSessions; 

        public SessionBoundary(IDocumentStore store, IContainer container)
        {
            _store = store;
            _otherSessions = new Cache<Type, IDocumentSession>(type => {
                var otherStore = container.ForGenericType(typeof (IDocumentStore<>))
                         .WithParameters(type)
                         .GetInstanceAs<IDocumentStore>();

                return otherStore.OpenSession();
            });

            reset();
        }

        private IEnumerable<IDocumentSession> openSessions()
        {
            if (_session != null && _session.IsValueCreated)
            {
                yield return _session.Value;
            }

            foreach (var session in _otherSessions)
            {
                yield return session;
            }
        } 

        public void Dispose()
        {
            WithOpenSession(s => s.Dispose());
            _otherSessions.ClearAll();
        }

        public IDocumentSession Session()
        {
            return _session.Value;
        }

        public IDocumentSession Session<T>() where T : RavenDbSettings
        {
            return _otherSessions[typeof (T)];
        }

        public void WithOpenSession(Action<IDocumentSession> action)
        {
            openSessions().Each(action);


        }

        public void SaveChanges()
        {
            WithOpenSession(s => s.SaveChanges());
        }

        public void Start()
        {
            reset();
        }

        public void MakeReadOnly()
        {
            // TODO -- figure out how to make the entire document store / session be readonly 
            // Nothing yet, dadgummit
        }

        private void reset()
        {
            WithOpenSession(s => s.Dispose());
            _session = new Lazy<IDocumentSession>(() =>
            {
                Debug.WriteLine("Opening a new DocumentSession");
                return _store.OpenSession();
            });
        }
    }
}