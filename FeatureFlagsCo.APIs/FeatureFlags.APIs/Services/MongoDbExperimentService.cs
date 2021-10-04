using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbExperimentService : MongoCollectionServiceBase<Experiment>
    {
        public MongoDbExperimentService(IMongoDbSettings settings) : base(settings) { }

        public async Task<List<Experiment>> GetByFeatureFlagAndEventAsync(string featureFlagId, string eventName)
        {
            return await _collection
                .Find(e => e.FlagId == featureFlagId && e.EventName == eventName && !e.IsArvhived).ToListAsync();
        }
    }
}
