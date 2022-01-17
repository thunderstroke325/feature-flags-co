using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentV2Service : ITransientDependency
    {
        private readonly MongoDbIntIdRepository<EnvironmentV2> _environments;
        private readonly IFeatureFlagsService _featureFlagsService;

        public EnvironmentV2Service(
            MongoDbIntIdRepository<EnvironmentV2> environments,
            IFeatureFlagsService featureFlagsService)
        {
            _environments = environments;
            _featureFlagsService = featureFlagsService;
        }

        public async Task<EnvironmentV2> GetAsync(int id)
        {
            var env = await _environments.FirstOrDefaultAsync(env => env.Id == id);
            if (env == null)
            {
                throw new EntityNotFoundException($"environment with id {id} was not found");
            }

            return env;
        }

        public async Task<EnvironmentV2> CreateAsync(
            int accountId,
            int projectId,
            string name,
            string description)
        {
            // add new environment
            var environment = new EnvironmentV2(projectId, name, description);
            await _environments.AddAsync(environment);

            // generate environment secret
            environment.GenerateSecrets(accountId);
            await _environments.UpdateAsync(environment);

            return environment;
        }

        public async Task<IEnumerable<EnvironmentV2>> CreateDefaultAsync(
            int accountId,
            int projectId,
            string creatorId,
            bool createDefaultFeatureFlag = false)
        {
            var prodEnv = await CreateAsync(accountId, projectId, "Production", "production");
            var testEnv = await CreateAsync(accountId, projectId, "Test", "test");

            var envs = new[] { prodEnv, testEnv };

            // create default feature flags
            if (createDefaultFeatureFlag)
            {
                foreach (var env in envs)
                {
                    await _featureFlagsService.CreateDefaultAsync(accountId, projectId, env.Id, creatorId);
                }
            }

            return envs;
        }

        public async Task<bool> DeleteAsync(int envId)
        {
            return await _environments.DeleteAsync(envId);
        }

        public async Task<EnvironmentV2> UpdateAsync(EnvironmentV2 updatedEnvironment)
        {
            return await _environments.UpdateAsync(updatedEnvironment);
        }

        public async Task<bool> DeleteAsync(Expression<Func<EnvironmentV2, bool>> filter)
        {
            return await _environments.DeleteAsync(filter);
        }
    }
}