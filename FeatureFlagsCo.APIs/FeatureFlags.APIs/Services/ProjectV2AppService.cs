using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;

namespace FeatureFlags.APIs.Services
{
    public class ProjectV2AppService : ITransientDependency
    {
        private readonly ProjectV2Service _projectService;
        private readonly AccountV2Service _accountService;
        private readonly EnvironmentV2Service _envService;

        public ProjectV2AppService(
            ProjectV2Service projectService,
            AccountV2Service accountService,
            EnvironmentV2Service envService)
        {
            _projectService = projectService;
            _accountService = accountService;
            _envService = envService;
        }

        public async Task<ProjectEnvironmentV2> CreateAsync(
            int accountId,
            string organizationName,
            string creatorId,
            bool createDefaultFeatureFlag = false)
        {
            // create default project for the account
            var project = await _projectService.CreateAsync(accountId, organizationName, creatorId);

            // create default environments for the project
            var environments =
                await _envService.CreateDefaultAsync(accountId, project.Id, creatorId, createDefaultFeatureFlag);

            var projectWithEnvs = new ProjectEnvironmentV2
            {
                Id = project.Id,
                Name = project.Name,
                Environments = environments
            };

            return projectWithEnvs;
        }

        public async Task<bool> DeleteAsync(Expression<Func<ProjectV2, bool>> filter)
        {
            var projects = await _projectService.GetListAsync(filter);
            var projectIds = projects.Select(project => project.Id);

            foreach (var projectId in projectIds)
            {
                await DeleteAsync(projectId);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int projectId)
        {
            // delete project & project users
            await _projectService.DeleteAsync(projectId);

            // delete all related environments
            await _envService.DeleteAsync(env => env.ProjectId == projectId);

            return true;
        }

        public async Task CheckPermissionAsync(
            int accountId,
            int projectId,
            string operation,
            string userId)
        {
            // only account owner/admin can create project
            if (operation == "create")
            {
                var isAccountOwnerOrAdmin = await _accountService.IsOwnerOrAdminAsync(accountId, userId);
                if (!isAccountOwnerOrAdmin)
                {
                    throw new PermissionDeniedException("only account owner/admin can create project");
                }
            }

            // only account owner/admin or project owner can update a project
            if (operation == "update")
            {
                var isAccountOwnerOrAdmin = await _accountService.IsOwnerOrAdminAsync(accountId, userId);
                var isProjectOwner = await _projectService.IsOwnerAsync(projectId, userId);

                if (!isProjectOwner && !isAccountOwnerOrAdmin)
                {
                    throw new PermissionDeniedException("account owner/admin or project owner can update a project");
                }
            }

            // only account owner or project owner can delete a project
            if (operation == "delete")
            {
                var isAccountOwner = await _accountService.IsOwnerAsync(accountId, userId);
                var isProjectOwner = await _projectService.IsOwnerAsync(projectId, userId);

                if (!isAccountOwner && !isProjectOwner)
                {
                    throw new PermissionDeniedException("only account owner or project owner can delete a project");
                }
            }
        }
    }
}