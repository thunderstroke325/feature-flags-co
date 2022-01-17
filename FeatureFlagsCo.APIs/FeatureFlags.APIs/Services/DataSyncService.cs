using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.ViewModels.DataSync;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IDataSyncService {
        Task<EnvironmentDataViewModel> GetEnvironmentDataAsync(int envId);
        Task SaveEnvironmentDataAsync(int envId, EnvironmentDataViewModel data);
    }

    public class DataSyncService : IDataSyncService
    {
        private readonly INoSqlService _noSqlService;
        private readonly IFeatureFlagsService _featureFlagService;

        public DataSyncService(
            INoSqlService noSqlService,
            IFeatureFlagsService featureFlagService)
        {
            _noSqlService = noSqlService;
            _featureFlagService = featureFlagService;
        }

        public async Task<EnvironmentDataViewModel> GetEnvironmentDataAsync(int envId)
        {
            return new EnvironmentDataViewModel
            {
                Date = DateTime.UtcNow,
                Version = "1.0",
                FeatureFlags = await _noSqlService.GetEnvironmentDataAsync<FeatureFlag>(envId),
                EnvironmentUsers = await _noSqlService.GetEnvironmentDataAsync<EnvironmentUser>(envId),
                EnvironmentUserProperties = (await _noSqlService.GetEnvironmentDataAsync<EnvironmentUserProperty>(envId)).FirstOrDefault()
            };
        }

        public async Task SaveEnvironmentDataAsync(int envId, EnvironmentDataViewModel data)
        {
            var envSecret = await _featureFlagService.GetEnvironmentSecretAsync(envId);
            await _noSqlService.SaveEnvironmentDataAsync(envSecret.AccountId, envSecret.ProjectId, envId, data);
        }
    }
}
