using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface INoSqlService
    {
        #region old version true false status functions
        Task TrueFalseStatusUpdateItemAsync(string id, dynamic item);
        Task<dynamic> TrueFalseStatusGetItemAsync(string id);
        Task<EnvironmentFeatureFlagUser> TrueFalseStatusAddEnvironmentFeatureFlagUserAsync(EnvironmentFeatureFlagUser item);
        Task<EnvironmentFeatureFlagUser> TrueFalseStatusGetEnvironmentFeatureFlagUserAsync(string id);
        Task<int> TrueFalseStatusGetFeatureFlagTotalUsersAsync(string featureFlagId);
        Task<int> TrueFalseStatusGetFeatureFlagHitUsersAsync(string featureFlagId);
        #endregion

        Task<List<T>> GetEnvironmentDataAsync<T>(int envId);
        Task SaveEnvironmentDataAsync(int accountId, int projectId, int envId, EnvironmentDataViewModel data);

        Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize);
        Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize);
        Task<EnvironmentUser> GetEnvironmentUserAsync(string id);
        Task<FeatureFlag> GetFeatureFlagAsync(string id);
        Task<EnvironmentUser> AddEnvironmentUserAsync(EnvironmentUser item);
        Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100);

        Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId, string searchText = "", int pageIndex = 0, int pageSize = 20);

        Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100);

        Task<FeatureFlag> GetFlagAsync(string id);
        Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId);
        Task CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param);

        Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId, int accountId);
        Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag param);
        Task<ReturnJsonModel<FeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(FeatureFlag param);
        Task<FeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);
        Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);

        Task<EnvironmentUserProperty> UpdateEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName);
        Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesAsync(int environmentId);

        Task UpsertEnvironmentUserAsync(EnvironmentUser param);


    }
}
