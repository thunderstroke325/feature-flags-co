using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureFlagService
    {
        private readonly IMongoCollection<FeatureFlag> _featureFlags;

        public MongoDbFeatureFlagService(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _featureFlags = database.GetCollection<FeatureFlag>("FeatureFlags");
        }

        public List<FeatureFlag> Get() =>
            _featureFlags.Find(p => p.ObjectType == "FeatureFlag").ToList();

        public async Task<List<FeatureFlag>> SearchAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText.Trim()))
                return await _featureFlags.Find(p => p.EnvironmentId == environmentId).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _featureFlags.Find(p => p.EnvironmentId == environmentId && p.FF.Name.Contains(searchText)).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();

            
        }

        public async Task<List<FeatureFlag>> SearchArchivedAsync(int environmentId, int pageIndex, int pageSize)
        {
            return await _featureFlags.Find(p => p.EnvironmentId == environmentId && p.IsArchived == true).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<FeatureFlag> GetAsync(string id) =>
            await _featureFlags.Find<FeatureFlag>(book => book.Id == id).FirstOrDefaultAsync();


        public async Task<FeatureFlag> CreateAsync(FeatureFlag book)
        {
            await _featureFlags.InsertOneAsync(book);
            return book;
        }

        public async Task UpdateAsync(string id, FeatureFlag featureFlag)
        {
            await _featureFlags.FindOneAndReplaceAsync(p => p.Id == id, featureFlag);
            //await _featureFlags.ReplaceOneAsync(p => p.Id == id, bookIn);
        }

        public async Task RemoveAsync(FeatureFlag bookIn) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == id);
    }
}
