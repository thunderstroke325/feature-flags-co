using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public abstract class MongoDbRepository<TEntity, TKey> : MongoDbPersist
        where TEntity : class, IEntity<TKey>
    {
        public IMongoCollection<TEntity> Collection { get; }

        protected string CollectionName => CollectionNameOf<TEntity>();

        public IMongoQueryable<TEntity> Queryable { get; }

        public MongoDbRepository(MongoClientAccessor clientAccessor) : base(clientAccessor)
        {
            Collection = CollectionOf<TEntity>();
            Queryable = QueryableOf<TEntity>();
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entity = await Queryable.FirstOrDefaultAsync(predicate);

            return entity;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await Collection.InsertOneAsync(entity);

            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity updatedEntity)
        {
            await Collection.FindOneAndReplaceAsync(entity => entity.Id.Equals(updatedEntity.Id), updatedEntity);

            return updatedEntity;
        }

        public virtual async Task<bool> UpdateOneAsync(
            Expression<Func<TEntity, bool>> filter,
            UpdateDefinition<TEntity> updateDefinition)
        {
            var result = await Collection.UpdateOneAsync(filter, updateDefinition);

            return result.IsAcknowledged;
        }

        public virtual async Task<bool> DeleteAsync(TKey key)
        {
            var result = await Collection.DeleteOneAsync(entity => entity.Id.Equals(key));

            return result.IsAcknowledged;
        }

        public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> filter)
        {
            var result = await Collection.DeleteManyAsync(filter);

            return result.IsAcknowledged;
        }
    }

    public class MongoDbObjectIdRepository<TEntity> : MongoDbRepository<TEntity, ObjectId>
        where TEntity : MongoDbObjectIdEntity
    {
        public MongoDbObjectIdRepository(MongoClientAccessor clientAccessor) : base(clientAccessor)
        {
        }
    }

    public class MongoDbIntIdRepository<TEntity> : MongoDbRepository<TEntity, int>
        where TEntity : MongoDbIntIdEntity
    {
        public MongoDbIntIdRepository(MongoClientAccessor clientAccessor) : base(clientAccessor)
        {
        }

        public override async Task<TEntity> AddAsync(TEntity entity)
        {
            var newId = await GetNextLongIdAsync();
            entity.SetId(newId);
            
            return await base.AddAsync(entity);
        }

        private async Task<int> GetNextLongIdAsync()
        {
            var filter = Builders<CollectionIdCounter>.Filter.Where(counter => counter.Id == CollectionName);
            var update = Builders<CollectionIdCounter>.Update.Inc(counter => counter.Sequence, 1);

            // return the updated document
            var options = new FindOneAndUpdateOptions<CollectionIdCounter>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updated = await CollectionOf<CollectionIdCounter>().FindOneAndUpdateAsync(filter, update, options);
            if (updated == null)
            {
                throw new MongoDbRepositoryException(
                    $"no counter found for collection {CollectionName}, " +
                    "please initialize it in collection CollectionIdCounter manually!");
            }
            
            return updated.Sequence;
        }
    }
}