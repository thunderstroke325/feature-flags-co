using FeatureFlags.APIs.ViewModels;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;
using FeatureFlagsCo.MQ;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbExperimentService : MongoCollectionServiceBase<Experiment>
    {
        public MongoDbExperimentService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on updatedAt
            var indexKeysDefinition = Builders<Experiment>.IndexKeys.Descending(m => m.CreatedAt);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<Experiment>(indexKeysDefinition))).Wait();
        }

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

        public async Task<List<Experiment>> GetActiveExperimentsByEnvAsync(int envId)
        {
            var expts = await _collection
                .Find(e => e.EnvId == envId && !e.IsArvhived && e.Iterations.Count > 0).SortByDescending(p => p.CreatedAt).ToListAsync();

            return expts.FindAll(ex => ex.Iterations.Find(it => !it.EndTime.HasValue) != null);
        }

        public async Task<List<Experiment>> GetExperimentsByMetricAsync(string metricId)
        {
            return await _collection
                .Find(e => e.MetricId == metricId && !e.IsArvhived).SortByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
