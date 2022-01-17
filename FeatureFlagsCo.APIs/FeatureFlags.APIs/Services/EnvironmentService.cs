using Microsoft.EntityFrameworkCore;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.ViewModels.Project;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Services.MongoDb;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public interface IEnvironmentService
    {
        public Task<IEnumerable<EnvironmentViewModel>> GetEnvs(int accountId, int projectId);

        // Remove all envs of a project
        // The currentUser must be the owner/admin of the account, or the owner/admin of the project, must be checked before calling this method
        public Task RemoveAllEnvsAsync(int projectId);

        // Remove a specific env of a project
        // The currentUser must be the owner/admin of the account, or owner/admin of the project, must be checked before calling this method
        public Task RemoveEnvAsync(int environmentId);

        public Task<EnvironmentViewModel> CreateEnvAsync(EnvironmentViewModel param, int accountId, string currentUserId, bool isInitializingAccount = false);

        Task<bool> CheckIfUserHasRightToReadEnvAsync(string userId, int envId);
    }

    public class EnvironmentService : IEnvironmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGenericRepository _repository;
        private readonly INoSqlService _noSqlDbService;
        private readonly IEnvironmentUserPropertyService _environmentService;
        private readonly MongoDbPersist _mongoDb;

        public EnvironmentService(
            ApplicationDbContext context,
            IGenericRepository repository,
            IEnvironmentUserPropertyService environmentService,
            INoSqlService noSqlDbService, 
            MongoDbPersist mongoDb)
        {
            _dbContext = context;
            _repository = repository;
            _environmentService = environmentService;
            _noSqlDbService = noSqlDbService;
            _mongoDb = mongoDb;
        }

        public async Task<IEnumerable<EnvironmentViewModel>> GetEnvs(int accountId, int projectId)
        {
            var query = from project in _dbContext.Projects
                        join env in _dbContext.Environments on project.Id equals env.ProjectId
                        where project.AccountId == accountId && project.Id == projectId
                        select new EnvironmentViewModel
                        {
                            ProjectId = project.Id,
                            Id = env.Id,
                            Name = env.Name,
                            Description = env.Description,
                            Secret = env.Secret,
                            MobileSecret = env.MobileSecret
                        };

            return await query.ToListAsync();
        }

        public async Task RemoveAllEnvsAsync(int projectId)
        {
            var envs = _dbContext.Environments.Where(x => x.ProjectId == projectId).ToList();

            if (envs != null && envs.Count > 0)
            {
                foreach (var env in envs)
                {
                    _dbContext.Environments.Remove(env);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveEnvAsync(int environmentId)
        {
            var env = _dbContext.Environments.Where(p => p.Id == environmentId).FirstOrDefault();
            if (env != null)
            {
                _dbContext.Environments.Remove(env);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<EnvironmentViewModel> CreateEnvAsync(EnvironmentViewModel param, int accountId, string currentUserId, bool isInitializingAccount = false) 
        {
            var env = await _repository.CreateAsync<Environment>(new Environment
            {
                ProjectId = param.ProjectId,
                Description = param.Description,
                MobileSecret = FeatureFlagKeyExtension.GenerateEnvironmentKey(param.Id, accountId, param.ProjectId, "mobile"),
                Name = param.Name,
                Secret = FeatureFlagKeyExtension.GenerateEnvironmentKey(param.Id, accountId, param.ProjectId)
            });

            // create demo FF
            if (isInitializingAccount) 
            {
                // set customized user properties
                var userProperty = new EnvironmentUserProperty
                {
                    EnvironmentId = env.Id,
                    Properties = new List<string> { "age" }
                };

                await _environmentService.CreateOrUpdateCosmosDBEnvironmentUserPropertiesForCRUDAsync(userProperty);

                var demoFFVM = new CreateFeatureFlagViewModel 
                { 
                    Name = "示例开关",
                    Status = "Enabled",
                    EnvironmentId = env.Id
                };

                await _noSqlDbService.CreateDemoFeatureFlagAsync(demoFFVM, currentUserId, param.ProjectId, accountId);
            }

            if(param.Id <= 0)
            {
                env.Secret = FeatureFlagKeyExtension.GenerateEnvironmentKey(env.Id, accountId, param.ProjectId);
                env.MobileSecret = FeatureFlagKeyExtension.GenerateEnvironmentKey(env.Id, accountId, param.ProjectId, "mobile");
                await _repository.UpdateAsync<Environment>(env);
            }

            return new EnvironmentViewModel
            {
                Id = env.Id,
                ProjectId = env.ProjectId,
                Description = env.Description,
                MobileSecret = env.MobileSecret,
                Name = env.Name,
                Secret = env.Secret
            };
        }


        public async Task<bool> CheckIfUserHasRightToReadEnvAsync(string userId, int envId)
        {
            // adapted to sqlserver & mongodb
            
            // check sqlserver
            var mssqlQuery =
                from project in _dbContext.Projects
                join env in _dbContext.Environments on project.Id equals env.ProjectId
                join accountUser in _dbContext.AccountUserMappings on project.AccountId equals accountUser.AccountId
                where env.Id == envId && accountUser.UserId == userId
                select accountUser;
            var mssqlResult = await mssqlQuery.FirstOrDefaultAsync();
            if (mssqlResult != null)
            {
                return true;
            }

            // check mongodb
            var mongoQuery =
                from project in _mongoDb.QueryableOf<ProjectV2>()
                join env in _mongoDb.QueryableOf<EnvironmentV2>() on project.Id equals env.ProjectId
                join accountUser in _mongoDb.QueryableOf<AccountUserV2>()
                    on project.AccountId equals accountUser.AccountId
                where env.Id == envId && accountUser.UserId == userId
                select accountUser;
            var mongoResult = await mongoQuery.FirstOrDefaultAsync();
            if (mongoResult != null)
            {
                return true;
            }

            return false;
        }
    }
}
