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
    [Route("api/accounts/{accountId}/projects/{projectId}/envs")]
    public class ProjectEnvironmentsController : ControllerBase
    {
        private readonly IGenericRepository _repository;
        private readonly ILogger<AccountsController> _logger;
        private readonly IAccountUserService _accountUserService;
        private readonly IProjectUserService _projectUserService;
        private readonly IEnvironmentService _environmentService;

        public ProjectEnvironmentsController(
            ILogger<AccountsController> logger,
            IGenericRepository repository,
            IAccountUserService accountUserService,
            IProjectUserService projectUserService,
            IEnvironmentService environmentService)
        {
            _logger = logger;
            _repository = repository;
            _accountUserService = accountUserService;
            _projectUserService = projectUserService;
            _environmentService = environmentService;
        }

        [HttpGet]
        [Route("")]
        public async Task<dynamic> GetEnvs(int accountId, int projectId)
        {
            // Only account owner/admin or project owner can update a project
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (!_accountUserService.IsAccountMember(accountId, currentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            return await _environmentService.GetEnvs(accountId, projectId);
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> CreateEnv(int accountId, int projectId, [FromBody] EnvironmentViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            var project = await _repository.SelectByIdAsync<Project>(projectId);

            if (
                project == null || 
                project.AccountId != accountId ||
                (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }) &&
                !_projectUserService.IsInProjectUserRoles(projectId, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
              )
            {
                return StatusCode (StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            if (string.IsNullOrWhiteSpace(param.Name))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad request" });
            }

            var newEnv = await _environmentService.CreateEnvAsync(param, accountId);
            EnvKeyViewModel envKeyViewModel = await RegenerateEnvKey(accountId, projectId, newEnv.Id, new EnvKeyViewModel
            {
                KeyName = "Secret"
            });
            newEnv.Secret = envKeyViewModel.KeyValue;
            return newEnv;
        }

        [HttpPut]
        [Route("")]
        public async Task<dynamic> UpdateEnv(int accountId, int projectId, [FromBody] EnvironmentViewModel param)
        {
            var env = await _repository.SelectByIdAsync<Environment>(param.Id);
            var project = await _repository.SelectByIdAsync<Project>(projectId);

            // Only account owner/admin or project owner can update a project
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (
                env == null ||
                env.ProjectId != project.Id ||
                project == null ||
                project.AccountId != accountId ||
                (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }) &&
                !_projectUserService.IsInProjectUserRoles(projectId, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
                )
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            if (string.IsNullOrWhiteSpace(param.Name))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad request" });
            }

            env.Name = param.Name;
            env.Description = param.Description;
            return await _repository.UpdateAsync<Environment>(env);
        }

        [HttpDelete]
        [Route("{envId}")]
        public async Task<dynamic> RemoveEnv(int accountId, int projectId, int envId)
        {
            var env = await _repository.SelectByIdAsync<Environment>(envId);
            var project = await _repository.SelectByIdAsync<Project>(projectId);

            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (
                env == null ||
                env.ProjectId != project.Id ||
                project == null ||
                project.AccountId != accountId ||
                (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }) &&
                !_projectUserService.IsInProjectUserRoles(projectId, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
                  )
            {
                return StatusCode (StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            await _environmentService.RemoveEnvAsync(envId);

            return Ok();
        }

        // reset key: secret, mobile secret or both
        [HttpPut]
        [Route("{envId}/key")]
        public async Task<dynamic> RegenerateEnvKey(int accountId, int projectId, int envId, [FromBody] EnvKeyViewModel param)
        {
            var env = await _repository.SelectByIdAsync<Environment>(envId);
            var project = await _repository.SelectByIdAsync<Project>(projectId);

            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (
                env == null ||
                env.ProjectId != project.Id ||
                project == null ||
                project.AccountId != accountId ||
                (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }) &&
                !_projectUserService.IsInProjectUserRoles(projectId, currentUserId, new List<ProjectUserRoleEnum> { ProjectUserRoleEnum.Owner }))
                    )
            {
                return StatusCode (StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            string newKey;
            switch (param.KeyName) 
            {
                case "Secret":
                    newKey = FeatureFlagKeyExtension.GenerateEnvironmentKey(envId, project.AccountId, projectId);
                    env.Secret = newKey;
                    break;
                case "MobileSecret":
                    newKey = FeatureFlagKeyExtension.GenerateEnvironmentKey(envId, project.AccountId, projectId, "mobile");
                    env.MobileSecret = newKey;
                    break;
                default:
                    return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message = "Bad request" });
            }
            
            await _repository.UpdateAsync<Environment>(env);
            return new EnvKeyViewModel 
            {
                KeyName = param.KeyName,
                KeyValue = newKey
            };
        }
    }
}
