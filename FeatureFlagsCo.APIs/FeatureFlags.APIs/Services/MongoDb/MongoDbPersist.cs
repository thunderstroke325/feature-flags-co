using System;
using System.Collections.Generic;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public class MongoDbPersist : ITransientDependency
    {
        private readonly Dictionary<Type, string> _collectionNameMap = new Dictionary<Type, string>
        {
            { typeof(CollectionIdCounter), "CollectionIdCounter" },
            { typeof(AccountV2), "Accounts" },
            { typeof(AccountUserV2), "AccountUsers" },
            { typeof(ProjectV2), "Projects" },
            { typeof(ProjectUserV2), "ProjectUsers" },
            { typeof(EnvironmentV2), "Environments" },
            { typeof(FeatureFlagTagTrees), "FeatureFlagTrees" },
            { typeof(FeatureFlag), "FeatureFlags" }, 
            { typeof(EnvironmentUser), "EnvironmentUsers" }, 
            { typeof(Segment), "Segments" }
        };

        public MongoClient Client { get; }

        public IMongoDatabase Database { get; }

        public MongoDbPersist(MongoClientAccessor clientAccessor)
        {
            Client = clientAccessor.MongoClient;
            
            var databaseName = clientAccessor.DatabaseName;
            Database = Client.GetDatabase(databaseName);
        }

        public IMongoCollection<TEntity> CollectionOf<TEntity>()
        {
            var collectionName = CollectionNameOf<TEntity>();
            var collection = Database.GetCollection<TEntity>(collectionName);
            return collection;
        }

        public string CollectionNameOf<TEntity>()
        {
            if (!_collectionNameMap.TryGetValue(typeof(TEntity), out var collectionName))
            {
                var exception = new MongoDbRepositoryException(
                    $"collection name of type {typeof(TEntity)} is not registered, " +
                    "please register in _collectionNameMap first.");

                throw exception;
            }

            return collectionName;
        }

        public IMongoQueryable<TEntity> QueryableOf<TEntity>()
        {
            return CollectionOf<TEntity>().AsQueryable();
        }
    }
}