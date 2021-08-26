using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.DataSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IDataSyncService {
        Task<EnvironmentDataViewModel> GetEnvironmentDataAsync(int envId, DownloadOptionEnum options);
        Task SaveEnvironmentDataAsync(int envId, UserUpdateModeEnum userUpdateMode, EnvironmentDataViewModel data);
    }

    public class DataSyncService : IDataSyncService
    {
        private readonly INoSqlService _noSqlService;
        public DataSyncService(INoSqlService noSqlService)
        {
            _noSqlService = noSqlService;
        }

        public async Task<EnvironmentDataViewModel> GetEnvironmentDataAsync(int envId, DownloadOptionEnum options)
        {
            var result = new EnvironmentDataViewModel
            {
                Date = DateTime.UtcNow,
                Version = "1.0",
                FeatureFlags = new List<FeatureFlag>(),
                EnvironmentUsers = new List<EnvironmentUser>()
            };

            if ((options & DownloadOptionEnum.FeatureFlags) != DownloadOptionEnum.None) 
            {
                result.FeatureFlags = await _noSqlService.GetEnvironmentDataAsync<FeatureFlag>(envId);
            }

            if ((options & DownloadOptionEnum.Users) != DownloadOptionEnum.None)
            {
                result.EnvironmentUsers = await _noSqlService.GetEnvironmentDataAsync<EnvironmentUser>(envId);
            }

            if ((options & DownloadOptionEnum.UserProperties) != DownloadOptionEnum.None)
            {
                result.EnvironmentUserProperties = (await _noSqlService.GetEnvironmentDataAsync<EnvironmentUserProperty>(envId)).FirstOrDefault();
            }

            return result;
        }

        public async Task SaveEnvironmentDataAsync(int envId, UserUpdateModeEnum userUpdateMode, EnvironmentDataViewModel data)
        {
            await _noSqlService.SaveEnvironmentDataAsync(envId, data);
        }
    }
}
