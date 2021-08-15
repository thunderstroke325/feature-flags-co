using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbEnvironmentUserPropertyService
    {
        private readonly IMongoCollection<EnvironmentUserProperty> _environmentUserProperties;

        public MongoDbEnvironmentUserPropertyService(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _environmentUserProperties = database.GetCollection<EnvironmentUserProperty>("EnvironmentUserProperties");
        }

        public List<EnvironmentUserProperty> Get() =>
            _environmentUserProperties.Find(p => p.ObjectType == "EnvironmentUserProperties").ToList();

        public async Task<EnvironmentUserProperty> GetAsync(string id) =>
            await _environmentUserProperties.Find<EnvironmentUserProperty>(book => book.Id == id).FirstOrDefaultAsync();

        public async Task<EnvironmentUserProperty> GetByEnvironmentIdAsync(int environmentId) =>
            await _environmentUserProperties.Find<EnvironmentUserProperty>(book => book.EnvironmentId == environmentId).FirstOrDefaultAsync();

        public async Task<EnvironmentUserProperty> CreateAsync(EnvironmentUserProperty book)
        {
            await _environmentUserProperties.InsertOneAsync(book);
            return book;
        }

        public async Task UpdateAsync(string id, EnvironmentUserProperty bookIn) =>
            await _environmentUserProperties.ReplaceOneAsync(book => book.Id == id, bookIn);

        public async Task RemoveAsync(EnvironmentUserProperty bookIn) =>
            await _environmentUserProperties.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _environmentUserProperties.DeleteOneAsync(book => book.Id == id);
    }
}
