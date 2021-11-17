using Microsoft.EntityFrameworkCore;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.ViewModels.Project;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.Services
{
    public interface IProjectService
    {

        public Task<IEnumerable<ProjectViewModel>> GetProjects(int accountId);

        Task<ProjectViewModel> CreateProjectAsync(string currentUserId, int accountId, ProjectViewModel param, bool isInitializingAccount = false);

        // Remove all projects of an account
        // The currentUser must be the owner/admin of the account, must be checked before calling this method
        public Task RemoveAllProjectsAsync(int accountId);

        // Remove a specific project of an account
        // The currentUser must be the owner/admin of the account, or owner of the project, must be checked before calling this method
        public Task RemoveProjectAsync(int projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGenericRepository _repository;
        private readonly IProjectUserService _projectUserService;
        private readonly IEnvironmentService _environmentService;

        public ProjectService(ApplicationDbContext context,
            IGenericRepository repository,
            IProjectUserService projectUserService,
            IEnvironmentService environmentService)
        {
            _dbContext = context;
            _repository = repository;
            _projectUserService = projectUserService;
            _environmentService = environmentService;
        }

        public async Task<IEnumerable<ProjectViewModel>> GetProjects(int accountId) 
        {
            var query = from project in _dbContext.Projects
                        join env in _dbContext.Environments on project.Id equals env.ProjectId into ps
                        from env in ps.DefaultIfEmpty() //left join
                        where project.AccountId == accountId
                        select new ProjectEnvironmentViewModel
                        {
                            ProjectId = project.Id,
                            ProjectName = project.Name,
                            EnvironmentId = env.Id,
                            EnvironmentName = env.Name,
                            EnvironmentDescription = env.Description,
                            EnvironmentSecret = env.Secret,
                            EnvironmentMobileSecret = env.MobileSecret
                        };

            var projectEnvs = await query.ToListAsync();

            return projectEnvs.GroupBy(
                p => p.ProjectId, 
                (key, g) => new ProjectViewModel
                {
                    Id = key,
                    Name = g.First().ProjectName,
                    Environments = g.Select(x => x.EnvironmentId == 0 ? null : new EnvironmentViewModel 
                    {
                        Id = x.EnvironmentId,
                        ProjectId = x.ProjectId,
                        Name = x.EnvironmentName,
                        Description = x.EnvironmentDescription,
                        MobileSecret = x.EnvironmentMobileSecret,
                        Secret = x.EnvironmentSecret
                    }).OfType<EnvironmentViewModel>() // exclude null environment
                }
            ).OrderByDescending(x => x.Id);
        }

        public async Task RemoveAllProjectsAsync(int accountId)
        {
            var projects = _dbContext.Projects.Where(x => x.AccountId == accountId).ToList();

            if (projects != null && projects.Count > 0)
            {
                foreach (var project in projects)
                {
                    await _environmentService.RemoveAllEnvsAsync(project.Id);
                    await _projectUserService.RemoveAllUsersAsync(project.Id);

                    _dbContext.Projects.Remove(project);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveProjectAsync(int projectId)
        {
            var project = _dbContext.Projects.Where(p => p.Id == projectId).FirstOrDefault();
            if (project != null)
            {
                await _environmentService.RemoveAllEnvsAsync(project.Id);
                await _projectUserService.RemoveAllUsersAsync(project.Id);

                _dbContext.Projects.Remove(project);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<ProjectViewModel> CreateProjectAsync(string currentUserId, int accountId, ProjectViewModel param, bool isInitializingAccount = false)
        {
            var newProject = await _repository.CreateAsync<Project>(new Project
            {
                AccountId = accountId,
                Name = param.Name
            });

            // set current user as the project owner
            await _repository.CreateAsync<ProjectUserMapping>(new ProjectUserMapping
            {
                Role = ProjectUserRoleEnum.Owner.ToString(),
                UserId = currentUserId,
                ProjectId = newProject.Id
            });

            // Create envs
            var envs = new List<EnvironmentViewModel>();

            var prodEnv = await _environmentService.CreateEnvAsync(new EnvironmentViewModel
            {
                ProjectId = newProject.Id,
                Description = "production",
                Name = "Production"
            }, accountId, currentUserId, isInitializingAccount);
            envs.Add(prodEnv);

            var testEnv = await _environmentService.CreateEnvAsync(new EnvironmentViewModel
            {
                ProjectId = newProject.Id,
                Description = "test",
                Name = "Test"
            }, accountId, currentUserId, isInitializingAccount);

            envs.Add(testEnv);

            return new ProjectViewModel
            {
                Id = newProject.Id,
                Name = newProject.Name,
                Environments = envs
            };
        }
    }
}
