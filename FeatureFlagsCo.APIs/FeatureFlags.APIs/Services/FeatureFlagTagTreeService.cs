using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class FeatureFlagTagTreeService : ITransientDependency
    {
        private readonly MongoDbObjectIdRepository<FeatureFlagTagTrees> _trees;

        public FeatureFlagTagTreeService(MongoDbObjectIdRepository<FeatureFlagTagTrees> trees)
        {
            _trees = trees;
        }

        public async Task<FeatureFlagTagTrees> FindAsync(int envId)
        {
            var trees = await _trees.FirstOrDefaultAsync(trees => trees.EnvId == envId);

            return trees;
        }

        public async Task<FeatureFlagTagTrees> GetAsync(int envId)
        {
            var trees = await FindAsync(envId);
            if (trees == null)
            {
                throw new EntityNotFoundException($"feature flag trees with envId {envId} not found");
            }

            return trees;
        }

        public async Task<FeatureFlagTagTrees> UpsertAsync(int envId, List<FeatureFlagTagTree> trees)
        {
            var featureFlagTrees = await _trees.FirstOrDefaultAsync(x => x.EnvId == envId);
            if (featureFlagTrees == null)
            {
                return await CreateAsync(envId, trees);
            }

            return await UpdateAsync(envId, trees);
        }

        public async Task<FeatureFlagTagTrees> CreateAsync(int envId, List<FeatureFlagTagTree> trees)
        {
            var featureFlagTrees = new FeatureFlagTagTrees(envId, trees);

            var created = await _trees.AddAsync(featureFlagTrees);
            return created;
        }

        public async Task<FeatureFlagTagTrees> UpdateAsync(int envId, List<FeatureFlagTagTree> updatedTrees)
        {
            var trees = await GetAsync(envId);

            trees.Update(updatedTrees);
            await _trees.UpdateAsync(trees);

            return trees;
        }
    }
}