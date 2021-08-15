using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Environment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Repositories
{
    public interface IEnvironmentUserPropertyService
    {

        Task<EnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(int environmentId);
        Task CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param);
    }

    public class EnvironmentUserPropertyService : IEnvironmentUserPropertyService
    {
        private readonly INoSqlService _cosmosdbService;

        public EnvironmentUserPropertyService(
            INoSqlService cosmosdbService)
        {
            _cosmosdbService = cosmosdbService;
        }

        public async Task<EnvironmentUserProperty> GetCosmosDBEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            return await _cosmosdbService.GetEnvironmentUserPropertiesForCRUDAsync(environmentId);
        }

        public async Task CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param)
        {
            await _cosmosdbService.CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(param);
        }
    }

}
