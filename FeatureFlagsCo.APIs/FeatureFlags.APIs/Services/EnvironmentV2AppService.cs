using System.Threading.Tasks;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentV2AppService : ITransientDependency
    {
        private readonly AccountV2Service _accountService;
        private readonly ProjectV2Service _projectService;
        private readonly EnvironmentV2Service _envService;
        private readonly MongoDbFeatureFlagZeroCodeSettingService _ffZeroCodeSettingSrv;

        public EnvironmentV2AppService(
            AccountV2Service accountService,
            ProjectV2Service projectService,
            EnvironmentV2Service envService, 
            MongoDbFeatureFlagZeroCodeSettingService ffZeroCodeSettingSrv)
        {
            _accountService = accountService;
            _projectService = projectService;
            _envService = envService;
            _ffZeroCodeSettingSrv = ffZeroCodeSettingSrv;
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
    }
}