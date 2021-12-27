using FeatureFlagsCo.Messaging.ViewModels;
using MongoDB.Driver;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ;

namespace FeatureFlagsCo.Messaging.Services
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

        public T Get(string id) =>
            _collection.Find<T>(book => book.Id == id).FirstOrDefault();

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

        public T UpsertItem(T item)
        {
            var existingItem = Get(item.Id);
            if (existingItem != null)
            {
                item.Id = existingItem.Id;
                return Update(item.Id, item);
            }
            else
            {
                return Create(item);
            }
        }

        public async Task<T> CreateAsync(T item)
        {
            await _collection.InsertOneAsync(item);
            return item;
        }

        public T Create(T item)
        {
            _collection.InsertOne(item);
            return item;
        }

        public async Task<T> UpdateAsync(string id, T item)
        {
            return await _collection.FindOneAndReplaceAsync(p => p.Id == id, item);
        }

        public T Update(string id, T item)
        {
            return _collection.FindOneAndReplace(p => p.Id == id, item);
        }

        public async Task RemoveAsync(T bookIn) =>
            await _collection.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(book => book.Id == id);
    }
}
