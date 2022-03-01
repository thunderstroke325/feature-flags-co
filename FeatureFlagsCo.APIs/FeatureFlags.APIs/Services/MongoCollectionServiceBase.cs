using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ;

namespace FeatureFlags.APIs.Services
{ 
    public class MongoCollectionServiceBase<T> where T: MongoModelBase, new()
    {
        protected readonly IMongoCollection<T> _collection;

        public MongoCollectionServiceBase(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _collection = database.GetCollection<T>(new T().GetCollectionName());
        }

        public async Task<T> GetAsync(string id) =>
            await _collection.Find<T>(book => book.Id == id).FirstOrDefaultAsync();


        public async Task<T> UpsertItemAsync(T item)
        {
            var existingItem = await GetAsync(item.Id);
            if (existingItem != null)
            {
                item.Id = existingItem.Id;
                return await UpdateAsync(item.Id, item);
            }
            else
            {
                return await CreateAsync(item);
            }
        }

        public async Task<T> CreateAsync(T item)
        {
            await _collection.InsertOneAsync(item);
            return item;
        }

        public async Task<T> UpdateAsync(string id, T item)
        {
            return await _collection.FindOneAndReplaceAsync(p => p.Id == id, item);
        }
        
        public async Task UpdateManyAsync(
            FilterDefinition<T> filterDefinition, 
            UpdateDefinition<T> updateDefinition)
        {
            await _collection.UpdateManyAsync(filterDefinition, updateDefinition);
        }
    }
}
