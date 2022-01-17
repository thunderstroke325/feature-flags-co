using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services.MongoDb;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Repositories
{
    public interface IFeatureFlagsService
    {
        Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId);
        Task<EnvironmentSecretV2> GetEnvironmentSecretAsync(int envId);

        Task CreateDefaultAsync(
            int accountId,
            int projectId,
            int envId,
            string creatorId
        );
    }

    public class FeatureFlagsService : IFeatureFlagsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly INoSqlService _cosmosDbService;
        private readonly MongoDbPersist _mongoDb;

        public FeatureFlagsService(
            ApplicationDbContext context,
            INoSqlService cosmosDbService, 
            MongoDbPersist mongoDb)
        {
            _dbContext = context;
            _cosmosDbService = cosmosDbService;
            _mongoDb = mongoDb;
        }


        public async Task<EnvironmentUserQueryResultViewModel> QueryEnvironmentFeatureFlagUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize, string currentUserId)
        {
            var users = await _cosmosDbService.QueryEnvironmentUsersAsync(searchText, environmentId, pageIndex, pageSize);
            int pageTotalNumber = await _cosmosDbService.QueryEnvironmentUsersCountAsync(searchText, environmentId, pageIndex, pageSize);
            return new EnvironmentUserQueryResultViewModel
            {
                Count = pageTotalNumber,
                Users = users
            };
        }

        public async Task<EnvironmentSecretV2> GetEnvironmentSecretAsync(int envId)
        {
            string envSecret;

            // adapted to sqlserver & mongodb
            var sqlserverEnv = await _dbContext.Environments.FirstOrDefaultAsync(x => x.Id == envId);
            if (sqlserverEnv != null)
            {
                envSecret = sqlserverEnv.Secret;
            }
            else
            {
                var mongoEnv = await _mongoDb.QueryableOf<EnvironmentV2>().FirstOrDefaultAsync(x => x.Id == envId);
                envSecret = mongoEnv.Secret;
            }

            var secret = EnvironmentSecretV2.Parse(envSecret);
            return secret;
        }

        public async Task CreateDefaultAsync(
            int accountId,
            int projectId,
            int envId,
            string creatorId)
        {
            // set customized user properties
            var userProperty = new EnvironmentUserProperty
            {
                EnvironmentId = envId,
                Properties = new List<string> { "age" }
            };

            await _cosmosDbService.CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(userProperty);

            var demoFeatureFlag = new CreateFeatureFlagViewModel
            {
                Name = "示例开关",
                Status = "Enabled",
                EnvironmentId = envId
            };

            await _cosmosDbService.CreateDemoFeatureFlagAsync(demoFeatureFlag, creatorId, projectId, accountId);
        }
    }

}
