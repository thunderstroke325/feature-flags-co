﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FeatureFlags.APIs.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using FeatureFlags.APIs.Controllers.Base;

namespace FeatureFlags.APIs.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accounts/{accountId}/members")]
    public class AccountMembersController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository _repository;
        private readonly IAccountService _accountService;
        private readonly IAccountUserService _accountUserService;

        public AccountMembersController(
            UserManager<ApplicationUser> userManager,
            IGenericRepository repository,
            IAccountService accountService,
            IAccountUserService accountUserService)
        {
            _userManager = userManager;
            _repository = repository;
            _accountService = accountService;
            _accountUserService = accountUserService;
        }

        [HttpGet]
        [Route("")]
        public async Task<List<AccountUserViewModel>> GetAccountMembers(int accountId)
        {
           var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

           return await _accountUserService.GetAccountMembersAsync(currentUserId, accountId);
        }

        [HttpGet]
        [Route("{searchText}")]
        public async Task<dynamic> SearchAccountMembers(int accountId, string searchText)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }))
            {
                return await _accountUserService.SearchAccountMemberAsync(accountId, searchText);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task<dynamic> RemoveAccountMember(int accountId, string userId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (currentUserId == userId || !_accountUserService.IsInAccountUserRoles(accountId, userId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin })) 
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            await _accountUserService.RemoveUserAsync(accountId, userId);

            return Ok();
        }

        [HttpPost]
        [Route("")]
        public async Task<dynamic> AddMemberToAccount(int accountId, [FromBody] AccountUserViewModel param)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;

            if (!_accountUserService.IsInAccountUserRoles(accountId, currentUserId, new List<AccountUserRoleEnum> { AccountUserRoleEnum.Owner, AccountUserRoleEnum.Admin }))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Code = "Error", Message = "Forbidden" });
            }

            var allowedInitialRoles = new List<string> { AccountUserRoleEnum.Admin.ToString() };
            var roleString = allowedInitialRoles.FirstOrDefault(x => x.Equals(param.Role));
            if (string.IsNullOrEmpty(roleString))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Code = "Error", Message ="Bad Request" });
            }

            var user = await _userManager.FindByEmailAsync(param.Email);
            if (user == null)
            {
                var password = MiscService.GeneratePassword(param.Email);
                var result = await _userManager.CreateAsync(
                    new ApplicationUser()
                    {
                        Email = param.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = param.Email,
                        EmailConfirmed = true
                    },
                    password
                );

                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Code = "Error", Message = result.Errors.ToList().First().Description });

                user = await _userManager.FindByEmailAsync(param.Email);

                var account = new AccountViewModel
                {
                    OrganizationName = "Default organization"
                };

                var newAccount = await _accountService.CreateAccountAsync(user.Id, account);
                await _repository.CreateAsync<UserInvitation>(new UserInvitation { UserId = user.Id, AccountId = newAccount.Id, InitialPassword = password });
            }

            await _repository.CreateAsync<AccountUserMapping>(new AccountUserMapping { AccountId = accountId , UserId = user.Id, Role = roleString, InvitorUserId = currentUserId });

            return Ok();
        }
    }
}