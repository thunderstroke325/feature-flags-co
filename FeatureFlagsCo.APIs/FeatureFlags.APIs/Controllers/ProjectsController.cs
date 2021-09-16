using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.ViewModels.Project;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.Account;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [Authorize]
    [ApiController]
    [Route("api/accounts/{accountId}/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<AccountsController> _logger;
        private readonly IAccountUserService _accountUserService;
        private readonly IProjectUserService _projectUserService;
        private readonly IProjectService _projectService;

        public ProjectsController(
            ILogger<AccountsController> logger,
            IGenericRepository repository,
            IAccountUserService accountUserService,
            IProjectUserService projectUserService,
            IProjectService projectService)
        {
            _logger = logger;
            _repository = repository;
            _accountUserService = accountUserService;
            _projectUserService = projectUserService;
            _projectService = projectService;
        }

        [HttpGet]
        [Route("")]
        public async Task<dynamic> GetProjects(int accountId)
        {
            // Only account owner/admin or project owner can update a project
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (!_accountUserService.IsAccountMember(accountId, currentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            return await _projectService.GetProjects(accountId);
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateProject(int accountId, [FromBody] ProjectViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin })) 
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            return await _projectService.CreateProjectAsync(currentUserId, accountId, param);
        }

        [HttpPut]
        [Route("")]
        public async Task<dynamic> UpdateProject(int accountId, [FromBody] ProjectViewModel param)
        {
            var project = await _repository.SelectByIdAsync<Project>(param.Id);

            if (project != null)
            {
                // Only account owner/admin or project owner can update a project
                var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

                if (
                    project.AccountId != accountId ||
                    (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }) &&
                    !_projectUserService.IsInProjectUserRoles(project.Id, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
                    )
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Code", Message = "Forbidden" });
                }

                project.Name = param.Name;
                return await _repository.UpdateAsync<Project>(project);
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpDelete]
        [Route("{projectId}")]
        public async Task<dynamic> RemoveProject(int accountId, int projectId)
        {
            var project = await _repository.SelectByIdAsync<Project>(projectId);
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (
                project.AccountId != accountId ||
                (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner }) &&
                !_projectUserService.IsInProjectUserRoles(project.Id, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
                )
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            await _projectService.RemoveProjectAsync(projectId);

            return Ok();
        }
    }
}
