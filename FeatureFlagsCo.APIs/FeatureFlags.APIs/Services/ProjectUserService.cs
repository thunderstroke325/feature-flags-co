using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.ViewModels.Project;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IProjectUserService
    {
        // Remove all users of a project
        // The currentUser must be the owner/admin of the account, or the owner of the project, must be checked before calling this method
        public Task RemoveAllUsersAsync(int projectId);

        public bool IsInProjectUserRoles(int projectId, string userId, IEnumerable<ProjectUserRoleEnum> roles);
    }

    public class ProjectUserService : IProjectUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectUserService(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task RemoveAllUsersAsync(int projectId)
        {
            var pums = _dbContext.ProjectUserMappings.Where(x => x.ProjectId == projectId).ToList();

            if (pums != null && pums.Count > 0)
            {
                foreach (var item in pums)
                {
                    _dbContext.ProjectUserMappings.Remove(item);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public bool IsInProjectUserRoles(int projectId, string userId, IEnumerable<ProjectUserRoleEnum> roles) 
        {
            var roleStrings = roles.Select(x => x.ToString());
            var projectUser = _dbContext
               .ProjectUserMappings
               .FirstOrDefault(p => p.ProjectId == projectId && p.UserId == userId && roleStrings.Contains(p.Role));

            return projectUser != null;
        }
    }
}
