using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureFlagHtmlDetectionSettingService : MongoCollectionServiceBase<FeatureFlagHtmlDetectionSetting>
    {
        public MongoDbFeatureFlagHtmlDetectionSettingService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on updatedAt
            var indexKeysDefinition = Builders<FeatureFlagHtmlDetectionSetting>.IndexKeys.Descending(m => m.EnvironmentKey);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<FeatureFlagHtmlDetectionSetting>(indexKeysDefinition))).Wait();
        }

        public async Task<List<FeatureFlagHtmlDetectionSetting>> GetFeatureFlagHtmlDetectionSettingsAsync(string environmentKey)
        {
            return await _collection
                .Find(e => e.EnvironmentKey == environmentKey && e.IsActive == true).ToListAsync();
        }

        public async Task<List<FeatureFlagHtmlDetectionSetting>> CheckIfElementExistAlreadyAsync(string featureFlagId)
        {
            return await _collection
                .Find(e => e.FeatureFlagId == featureFlagId).ToListAsync();
        }
    }
}
