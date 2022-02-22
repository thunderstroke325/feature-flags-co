using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentV2Service : ITransientDependency
    {
        private readonly MongoDbIntIdRepository<EnvironmentV2> _environments;

        public EnvironmentV2Service(MongoDbIntIdRepository<EnvironmentV2> environments)
        {
            _environments = environments;
        }

        public async Task<EnvironmentV2> GetAsync(int id)
        {
            var env = await FindAsync(id);
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

        public async Task<EnvironmentV2> FindAsync(int envId)
        {
            var env = await _environments.FirstOrDefaultAsync(env => env.Id == envId);
            
            return env;
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