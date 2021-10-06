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

        public async Task<List<Experiment>> GetByIdsAsync(List<string> featureFlagIds)
        {
            return await _collection
                .Find(e => featureFlagIds.Contains(e.Id) && !e.IsArvhived).ToListAsync();
        }

        public async Task<List<Experiment>> GetByFeatureFlagAndMetricAsync(string featureFlagId, string metricId)
        {
            return await _collection
                .Find(e => e.FlagId == featureFlagId && e.MetricId == metricId && !e.IsArvhived).ToListAsync();
        }

        public async Task<List<Experiment>> GetByFeatureFlagAsync(string featureFlagId)
        {
            return await _collection
                .Find(e => e.FlagId == featureFlagId && !e.IsArvhived).SortByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
