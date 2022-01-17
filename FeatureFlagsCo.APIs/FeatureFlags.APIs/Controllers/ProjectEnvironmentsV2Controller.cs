using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels.Project;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/accounts/{accountId}/projects/{projectId}/envs")]
    public class ProjectEnvironmentsV2Controller : ApiControllerBase
    {
        private readonly EnvironmentV2Service _envService;
        private readonly EnvironmentV2AppService _envAppService;
        private readonly MongoDbPersist _mongoDb;
        private readonly IMapper _objectMapper;

        public ProjectEnvironmentsV2Controller(
            EnvironmentV2Service envService,
            EnvironmentV2AppService envAppService,
            MongoDbPersist mongoDb,
            IMapper objectMapper)
        {
            _envService = envService;
            _envAppService = envAppService;
            _mongoDb = mongoDb;
            _objectMapper = objectMapper;
        }

        [HttpGet]
        public async Task<List<EnvironmentViewModel>> GetListAsync(int accountId, int projectId)
        {
            var projects = _mongoDb.QueryableOf<ProjectV2>();
            var envs = _mongoDb.QueryableOf<EnvironmentV2>();

            var query =
                from project in projects
                join env in envs on project.Id equals env.ProjectId
                where project.Id == projectId && project.AccountId == accountId
                select new EnvironmentViewModel
                {
                    ProjectId = project.Id,
                    Id = env.Id,
                    Name = env.Name,
                    Description = env.Description,
                    Secret = env.Secret,
                    MobileSecret = env.MobileSecret
                };

            var result = await query.ToListAsync();
            return result;
        }

        [HttpPost]
        public async Task<EnvironmentViewModel> CreateAsync(
            int accountId,
            int projectId,
            [FromBody] EnvironmentViewModel input)
        {
            await CheckPermissionAsync(accountId, projectId);

            var env = await _envService.CreateAsync(accountId, projectId, input.Name, input.Description);

            var vm = _objectMapper.Map<EnvironmentV2, EnvironmentViewModel>(env);
            return vm;
        }

        [HttpPut]
        public async Task<EnvironmentV2> UpdateAsync(
            int accountId,
            int projectId,
            [FromBody] EnvironmentViewModel input)
        {
            await CheckPermissionAsync(accountId, projectId);

            var env = await _envService.GetAsync(input.Id);
            env.Update(input.Name, input.Description);

            var updated = await _envService.UpdateAsync(env);
            return updated;
        }

        [HttpDelete("{envId}")]
        public async Task<bool> DeleteAsync(int accountId, int projectId, int envId)
        {
            await CheckPermissionAsync(accountId, projectId);

            var deleteEnv = await _envService.DeleteAsync(envId);
            return deleteEnv;
        }

        [HttpPut]
        [Route("{envId}/key")]
        public async Task<EnvKeyViewModel> RegenerateKey(
            int accountId,
            int projectId,
            int envId,
            [FromBody] EnvKeyViewModel input)
        {
            await CheckPermissionAsync(accountId, projectId);
            
            var newSecret = await _envAppService.RegenerateSecretAsync(envId, accountId, input.KeyName, input.KeyValue);

            return new EnvKeyViewModel
            {
                KeyName = input.KeyName,
                KeyValue = newSecret
            };
        }

        private async Task CheckPermissionAsync(int accountId, int projectId)
        {
            await _envAppService.CheckPermissionAsync(accountId, projectId, CurrentUserId);
        }
    }
}