using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.APIs.ViewModels.Project;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.Exceptions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class ProjectV2Service : ITransientDependency
    {
        private readonly MongoDbIntIdRepository<ProjectV2> _projects;
        private readonly MongoDbIntIdRepository<ProjectUserV2> _projectUsers;

        public ProjectV2Service(
            MongoDbIntIdRepository<ProjectV2> projects,
            MongoDbIntIdRepository<ProjectUserV2> projectUsers)
        {
            _projects = projects;
            _projectUsers = projectUsers;
        }

        public async Task<ProjectV2> GetAsync(int id)
        {
            var project = await _projects.FirstOrDefaultAsync(x => x.Id == id);
            if (project == null)
            {
                throw new EntityNotFoundException($"project entity with id {id} was not found");
            }

            return project;
        }

        public async Task<IEnumerable<ProjectV2>> GetListAsync(Expression<Func<ProjectV2, bool>> filter)
        {
            var projects = await _projects.Queryable.Where(filter).ToListAsync();

            return projects;
        }

        public async Task<ProjectV2> CreateAsync(
            int accountId,
            string projectName,
            string creatorId)
        {
            // add new project
            var project = new ProjectV2(accountId, projectName);
            await _projects.AddAsync(project);

            // set current user as the project owner
            var projectUser = new ProjectUserV2(project.Id, creatorId, ProjectUserRoleEnum.Owner.ToString());
            await _projectUsers.AddAsync(projectUser);

            return project;
        }

        public async Task<ProjectV2> UpdateAsync(ProjectV2 updatedProject)
        {
            return await _projects.UpdateAsync(updatedProject);
        }

        public async Task<bool> DeleteAsync(int projectId)
        {
            // delete project
            var deleteProject = await _projects.DeleteAsync(projectId);

            // delete project user
            var deleteProjectUser =
                await _projectUsers.DeleteAsync(projectUser => projectUser.ProjectId == projectId);

            return deleteProject & deleteProjectUser;
        }

        public async Task<bool> IsOwnerAsync(int projectId, string userId)
        {
            var projectUser = await _projectUsers.FirstOrDefaultAsync(projectUser =>
                projectUser.ProjectId == projectId &&
                projectUser.UserId == userId
            );

            return projectUser != null && projectUser.Role == ProjectUserRoleEnum.Owner.ToString();
        }
    }
}