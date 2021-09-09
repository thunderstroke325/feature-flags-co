using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class FFPendingChangesService: MongoCollectionServiceBase<FFPendingChanges>
    {
        public FFPendingChangesService(IMongoDbSettings settings): base(settings) {}

        public async Task<List<FFPendingChanges>> GetByFFIdAsync(string featureFlagId)
        {
            return await _collection.Find(p => p.FeatureFlagId == featureFlagId).ToListAsync();
        }
    }
}
