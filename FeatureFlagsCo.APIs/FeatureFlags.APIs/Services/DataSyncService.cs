using FeatureFlags.APIs.Models;
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
        private readonly IEnvironmentService _envService;

        public DataSyncService(
            INoSqlService noSqlService,
            IEnvironmentService envService)
        {
            _noSqlService = noSqlService;
            _envService = envService;
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
            var envSecret = await _envService.GetSecretAsync(envId);
            await _noSqlService.SaveEnvironmentDataAsync(envSecret.AccountId, envSecret.ProjectId, envId, data);
        }
    }
}
