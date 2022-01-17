using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Controllers.Base;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Account;
using FeatureFlags.Utils.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/accounts/{accountId}/members")]
    public class AccountMemberV2Controller : ApiControllerBase
    {
        private readonly AccountV2Service _accountService;
        private readonly AccountV2AppService _accountAppService;
        private readonly ApplicationDbContext _sqlserver;

        public AccountMemberV2Controller(
            AccountV2Service accountService,
            AccountV2AppService accountAppService,
            ApplicationDbContext sqlserver)
        {
            _accountService = accountService;
            _sqlserver = sqlserver;
            _accountAppService = accountAppService;
        }

        [HttpGet]
        public async Task<IEnumerable<AccountUserViewModel>> GetListAsync(int accountId)
        {
            var accountUsers = await _accountService.GetUsersAsync(accountId);

            var userIds = accountUsers.Select(accountUser => accountUser.UserId);
            var identityUsers = await _sqlserver.Users.Where(user => userIds.Contains(user.Id)).ToListAsync();

            var isAccountOwner = await _accountService.IsOwnerAsync(accountId, CurrentUserId);

            var vms =
                from accountUser in accountUsers
                join identityUser in identityUsers on accountUser.UserId equals identityUser.Id
                select new AccountUserViewModel
                {
                    UserId = identityUser.Id,
                    Email = identityUser.Email,
                    UserName = identityUser.UserName,
                    Role = accountUser.Role,

                    // only project owner or invitor can view the initial password 
                    InitialPassword = isAccountOwner || CurrentUserId.Equals(accountUser.InvitorUserId)
                        ? accountUser.InitialPassword
                        : null
                };
            return vms;
        }

        [HttpGet]
        [Route("{searchText}")]
        public async Task<IEnumerable<AccountUserViewModel>> SearchAsync(int accountId, string searchText)
        {
            await CheckPermissionAsync(accountId);

            var vms = await GetListAsync(accountId);

            var filteredVms = vms.Where(vm => vm.Email.Contains(searchText) || vm.UserName.Contains(searchText));
            return filteredVms;
        }

        [HttpPost]
        public async Task<bool> CreateAsync(int accountId, [FromBody] AccountUserViewModel input)
        {
            // check permission
            await CheckPermissionAsync(accountId);

            // check role
            var allowedInitialRoles = new List<string> { AccountUserRoleEnum.Admin.ToString() };
            if (!allowedInitialRoles.Contains(input.Role))
            {
                throw new ArgumentException($"use {input.Role} as initial role is not allowed");
            }

            var success = await _accountAppService.InviteUserAsync(accountId, input.Email, input.Role, CurrentUserId);
            return success;
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task<bool> DeleteAsync(int accountId, string userId)
        {
            await CheckPermissionAsync(accountId);

            var deleteSuccess = await _accountService.DeleteUserAsync(accountId, userId);
            return deleteSuccess;
        }

        async Task CheckPermissionAsync(int accountId)
        {
            var isAccountOwnerOrAdmin =
                await _accountService.IsOwnerOrAdminAsync(accountId, CurrentUserId);
            if (!isAccountOwnerOrAdmin)
            {
                throw new PermissionDeniedException("only account owner/admin can search/invite/delete users");
            }
        }
    }
}