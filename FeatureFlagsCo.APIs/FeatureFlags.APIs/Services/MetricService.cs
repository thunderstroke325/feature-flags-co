using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace FeatureFlags.APIs.Services
{
    public class MetricService : MongoCollectionServiceBase<Metric>
    {
        public MetricService(IMongoDbSettings settings) : base(settings) 
        {
            // Create index on updatedAt
            var indexKeysDefinition = Builders<Metric>.IndexKeys.Descending(m => m.CreatedAt);
            Task.Run(() => _collection.Indexes.CreateOneAsync(new CreateIndexModel<Metric>(indexKeysDefinition))).Wait();
        }

        public async Task<List<Metric>> GetMetricsAsync(int envId, string searchText, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _collection.Find(p => p.EnvId == envId).SortByDescending(p => p.UpdatedAt).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _collection.Find(p => p.EnvId == envId && p.Name.ToLower().Contains(searchText.ToLower())).SortByDescending(p => p.CreatedAt).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<List<Metric>> GetMetricsByIdsAsync(IEnumerable<string> ids)
        {
           return await _collection.Find(p => ids.Contains(p.Id)).SortByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
