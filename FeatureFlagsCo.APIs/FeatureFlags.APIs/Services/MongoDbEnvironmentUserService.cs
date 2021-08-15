using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbEnvironmentUserService
    {
        private readonly IMongoCollection<EnvironmentUser> _environmentUsers;

        public MongoDbEnvironmentUserService(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _environmentUsers = database.GetCollection<EnvironmentUser>("EnvironmentUsers");
        }

        public List<EnvironmentUser> Get() =>
            _environmentUsers.Find(p => p.ObjectType == "EnvironmentUser").ToList();

        public async Task<EnvironmentUser> GetAsync(string id) =>
            await _environmentUsers.Find<EnvironmentUser>(book => book.Id == id).FirstOrDefaultAsync();

        public async Task<EnvironmentUser> CreateAsync(EnvironmentUser book)
        {
            await _environmentUsers.InsertOneAsync(book);
            return book;
        }

        public async Task UpsertAsync(EnvironmentUser eu)
        {
            if (string.IsNullOrWhiteSpace(eu._Id))
                await this.CreateAsync(eu);
            else
                await this.UpdateAsync(eu.Id, eu);
        }

        public async Task UpdateAsync(string id, EnvironmentUser bookIn) =>
            await _environmentUsers.ReplaceOneAsync(book => book.Id == id, bookIn);

        public async Task RemoveAsync(EnvironmentUser bookIn) =>
            await _environmentUsers.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _environmentUsers.DeleteOneAsync(book => book.Id == id);

        public async Task<int> CountAsync(string searchText, int environmentId)
        {
            if(string.IsNullOrWhiteSpace(searchText.Trim()))
                return (int)await _environmentUsers.CountDocumentsAsync(p => p.EnvironmentId == environmentId);
            else
                return (int)await _environmentUsers.CountDocumentsAsync(p => p.EnvironmentId == environmentId && p.Name.Contains(searchText));
        }

        public async Task<List<EnvironmentUser>> SearchAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText.Trim()))
                return await _environmentUsers.Find(p => p.EnvironmentId == environmentId).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _environmentUsers.Find(p => p.EnvironmentId == environmentId && p.Name.Contains(searchText)).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }
    }
}
