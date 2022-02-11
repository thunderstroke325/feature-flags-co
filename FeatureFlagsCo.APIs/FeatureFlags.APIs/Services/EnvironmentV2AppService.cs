using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentV2AppService : ITransientDependency
    {
        private readonly AccountV2Service _accountService;
        private readonly ProjectV2Service _projectService;
        private readonly EnvironmentV2Service _envService;
        private readonly FeatureFlagV2Service _featureFlagService;
        private readonly MongoDbFeatureFlagZeroCodeSettingService _ffZeroCodeSettingSrv;

        public EnvironmentV2AppService(
            AccountV2Service accountService,
            ProjectV2Service projectService,
            EnvironmentV2Service envService, 
            FeatureFlagV2Service featureFlagService, 
            MongoDbFeatureFlagZeroCodeSettingService ffZeroCodeSettingSrv)
        {
            _accountService = accountService;
            _projectService = projectService;
            _envService = envService;
            _ffZeroCodeSettingSrv = ffZeroCodeSettingSrv;
            _featureFlagService = featureFlagService;
        }

        public async Task CheckPermissionAsync(int accountId, int projectId, string userId)
        {
            var isAccountOwnerOrAdmin =
                await _accountService.IsOwnerOrAdminAsync(accountId, userId);
            var isProjectOwner = await _projectService.IsOwnerAsync(projectId, userId);

            if (!isAccountOwnerOrAdmin || !isProjectOwner)
            {
                throw new PermissionDeniedException(
                    "only the owner of this project can create/update/delete environments, you have no permission");
            }
        }

        public async Task<string> RegenerateSecretAsync(
            int envId,
            int accountId,
            string keyType, 
            string oldSecret)
        {
            // update the env
            var env = await _envService.GetAsync(envId);
            var newSecret = env.RegenerateSecret(keyType, accountId);
            await _envService.UpdateAsync(env);
            
            // sync update FeatureFlagZeroCodeSetting EnvSecret
            await _ffZeroCodeSettingSrv.UpdateEnvSecretAsync(oldSecret, newSecret);

            return newSecret;
        }
        
        public async Task<IEnumerable<EnvironmentV2>> CreateDefaultAsync(
            int accountId,
            int projectId,
            string creatorId,
            bool createDefaultFeatureFlag = false)
        {
            var prodEnv = await _envService.CreateAsync(accountId, projectId, "Production", "production");
            var testEnv = await _envService.CreateAsync(accountId, projectId, "Test", "test");

            var envs = new[] { prodEnv, testEnv };

            // create default feature flags
            if (createDefaultFeatureFlag)
            {
                foreach (var env in envs)
                {
                    await _featureFlagService.CreateDefaultAsync(accountId, projectId, env.Id, creatorId);
                }
            }

            return envs;
        }
    }
}