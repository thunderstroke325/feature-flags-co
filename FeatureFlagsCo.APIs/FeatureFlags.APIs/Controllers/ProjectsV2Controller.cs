using System;
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
    [Route("api/v{version:apiVersion}/accounts/{accountId}/projects")]
    public class ProjectsV2Controller : ApiControllerBase
    {
        private readonly ProjectV2Service _projectService;
        private readonly ProjectV2AppService _projectAppService;
        private readonly MongoDbPersist _mongoDb;
        private readonly IMapper _objectMapper;

        public ProjectsV2Controller(
            ProjectV2Service projectService,
            ProjectV2AppService projectAppService,
            MongoDbPersist mongoDb,
            IMapper objectMapper)
        {
            _projectService = projectService;
            _projectAppService = projectAppService;
            _mongoDb = mongoDb;
            _objectMapper = objectMapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProjectViewModel>> GetListAsync(int accountId)
        {
            var projects = _mongoDb.QueryableOf<ProjectV2>().Where(x => x.AccountId == accountId);
            var envs = _mongoDb.QueryableOf<EnvironmentV2>();

            var query =
                from project in projects
                join env in envs on project.Id equals env.ProjectId into allEnvs
                orderby project.Id descending
                select new ProjectEnvironmentV2
                {
                    Id = project.Id,
                    Name = project.Name,
                    Environments = allEnvs
                };

            var projectEnvs = await query.ToListAsync();

            var vm = _objectMapper.Map
                <IEnumerable<ProjectEnvironmentV2>, IEnumerable<ProjectViewModel>>(projectEnvs);

            return vm;
        }

        [HttpPost]
        public async Task<ProjectViewModel> CreateAsync(int accountId, [FromBody] ProjectViewModel input)
        {
            await CheckPermissionAsync(accountId, 0, "create");

            var projectEnv = await _projectAppService.CreateAsync(accountId, input.Name, CurrentUserId);

            var vm = _objectMapper.Map<ProjectEnvironmentV2, ProjectViewModel>(projectEnv);
            return vm;
        }

        [HttpPut]
        public async Task<ProjectV2> UpdateAsync(int accountId, [FromBody] ProjectViewModel input)
        {
            var projectId = input.Id;
            var project = await _projectService.GetAsync(projectId);
            if (project.AccountId != accountId)
            {
                throw new ArgumentException("accountId and projectId not match");
            }

            await CheckPermissionAsync(accountId, projectId, "update");

            project.Update(input.Name);

            var updated = await _projectService.UpdateAsync(project);
            return updated;
        }

        [HttpDelete]
        [Route("{projectId}")]
        public async Task<bool> DeleteAsync(int accountId, int projectId)
        {
            var project = await _projectService.GetAsync(projectId);
            if (project.AccountId != accountId)
            {
                throw new ArgumentException("accountId and projectId not match");
            }
            
            await CheckPermissionAsync(accountId, projectId, "delete");

            var success = await _projectAppService.DeleteAsync(projectId);
            return success;
        }

        async Task CheckPermissionAsync(
            int accountId,
            int projectId,
            string operation)
        {
            await _projectAppService.CheckPermissionAsync(accountId, projectId, operation, CurrentUserId);
        }
    }
}