using FeatureFlags.APIs.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbEnvironmentUserService
    {
        private readonly IMongoCollection<EnvironmentUser> _environmentUsers;

        public MongoDbEnvironmentUserService(MongoDbPersist mongoDb)
        {
            _environmentUsers = mongoDb.CollectionOf<EnvironmentUser>();
        }

        public async Task UpsertItemAsync(EnvironmentUser item)
        {
            var existingItem = await GetAsync(item.Id);
            if (existingItem != null)
            {
                item._Id = existingItem._Id;
                await UpdateAsync(item.Id, item);
            }
            else
            {
                await CreateAsync(item);
            }
        }

        public async Task<EnvironmentUser> GetAsync(string id) =>
            await _environmentUsers.Find(book => book.Id == id).FirstOrDefaultAsync();

        public async Task<EnvironmentUser> CreateAsync(EnvironmentUser book)
        {
            await _environmentUsers.InsertOneAsync(book);
            return book;
        }

        public async Task<List<EnvironmentUser>> GetByEnvironmentAsync(int envId)
        {
            return await _environmentUsers.Find(p => p.EnvironmentId == envId).ToListAsync();
        }

        public async Task UpsertAsync(EnvironmentUser eu)
        {
            if (string.IsNullOrWhiteSpace(eu._Id))
                await CreateAsync(eu);
            else
                await UpdateAsync(eu.Id, eu);
        }

        public async Task UpdateAsync(string id, EnvironmentUser bookIn) =>
            await _environmentUsers.ReplaceOneAsync(book => book.Id == id, bookIn);

        public async Task<int> CountAsync(string searchText, int environmentId)
        {
            if(string.IsNullOrWhiteSpace(searchText))
                return (int)await _environmentUsers.CountDocumentsAsync(p => p.EnvironmentId == environmentId);
            else
                return (int)await _environmentUsers.CountDocumentsAsync(p => p.EnvironmentId == environmentId && p.Name.Contains(searchText));
        }

        public async Task<List<EnvironmentUser>> SearchAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _environmentUsers.Find(p => p.EnvironmentId == environmentId).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _environmentUsers.Find(p => p.EnvironmentId == environmentId && p.Name.Contains(searchText)).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }
    }
}
