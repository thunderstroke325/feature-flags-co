using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class SegmentAppService : ITransientDependency
    {
        private readonly MongoDbPersist _mongoDb;

        public SegmentAppService(MongoDbPersist mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public async Task<IEnumerable<FlagSegmentReference>> GetFlagSegmentReferencesAsync(int envId, string segmentId)
        {
            var flags = await _mongoDb.QueryableOf<FeatureFlag>()
                .Where(flag =>
                    flag.EnvironmentId == envId &&
                    flag.FFTUWMTR.Any(rule => rule.RuleId == segmentId)
                )
                .ToListAsync();

            var references = flags.Select(x => new FlagSegmentReference
            {
                FlagName = x.FF.Name,
                FlagKeyName = x.FF.KeyName
            });

            return references;
        }
    }
}