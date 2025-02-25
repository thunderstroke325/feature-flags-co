﻿using FeatureFlags.APIs.Models;
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
            // Create index on EnvSecret
            var indexKeysDefinition = Builders<FeatureFlagZeroCodeSetting>.IndexKeys.Descending(m => m.EnvSecret);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<FeatureFlagZeroCodeSetting>(indexKeysDefinition))).Wait();
        }

        public async Task<List<FeatureFlagZeroCodeSetting>> GetByEnvSecretAsync(string envSecret)
        {
            return await _collection
                .Find(e => e.EnvSecret == envSecret && e.IsActive == true && !e.IsArchived && e.Items.Count > 0).ToListAsync();
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

        /// <summary>
        /// 更新零代码设置的 env secret
        /// </summary>
        /// <param name="oldSecret">旧 secret</param>
        /// <param name="newSecret">新 secret</param>
        public async Task UpdateEnvSecretAsync(string oldSecret, string newSecret)
        {
            var filter = Builders<FeatureFlagZeroCodeSetting>.Filter.Eq(setting => setting.EnvSecret, oldSecret);
            
            var updateDefinition =
                Builders<FeatureFlagZeroCodeSetting>.Update
                    .Set(setting => setting.EnvSecret, newSecret)
                    .Set(setting => setting.UpdatedAt, DateTime.UtcNow);
            
            await UpdateManyAsync(filter, updateDefinition);
        }
    }
}
