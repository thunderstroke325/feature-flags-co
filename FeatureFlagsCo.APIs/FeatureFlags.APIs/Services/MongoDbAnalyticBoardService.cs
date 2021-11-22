using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbAnalyticBoardService : MongoCollectionServiceBase<AnalyticBoard>
    {
        public MongoDbAnalyticBoardService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on EnvSecret
            var indexKeysDefinition = Builders<AnalyticBoard>.IndexKeys.Descending(m => m.EnvId);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<AnalyticBoard>(indexKeysDefinition))).Wait();
        }

        public async Task<AnalyticBoard> GetByEnvIdAsync(int envId)
        {
            return await _collection.Find(e => e.EnvId == envId).FirstOrDefaultAsync();
        }
    }
}
