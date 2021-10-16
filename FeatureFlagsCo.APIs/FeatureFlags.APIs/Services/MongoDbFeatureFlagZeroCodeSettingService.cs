using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureFlagZeroCodeSettingService : MongoCollectionServiceBase<FeatureFlagZeroCodeSetting>
    {
        public MongoDbFeatureFlagZeroCodeSettingService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on updatedAt
            var indexKeysDefinition = Builders<FeatureFlagZeroCodeSetting>.IndexKeys.Descending(m => m.EnvSecret);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<FeatureFlagZeroCodeSetting>(indexKeysDefinition))).Wait();
        }

        public async Task<List<FeatureFlagZeroCodeSetting>> GetByEnvSecretAsync(string envSecret)
        {
            return await _collection
                .Find(e => e.EnvSecret == envSecret && e.IsActive == true).ToListAsync();
        }

        public async Task<FeatureFlagZeroCodeSetting> GetByEnvAndFeatureFlagIdAsync(int envId, string ffId)
        {
            return await _collection
                .Find(e => e.EnvId == envId && e.FeatureFlagId == ffId && e.IsActive == true).FirstOrDefaultAsync();
        }

        public async Task<List<FeatureFlagZeroCodeSetting>> CheckIfElementExistAlreadyAsync(string featureFlagId)
        {
            return await _collection
                .Find(e => e.FeatureFlagId == featureFlagId).ToListAsync();
        }
    }
}
